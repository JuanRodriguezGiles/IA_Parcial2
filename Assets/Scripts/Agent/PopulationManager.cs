using UnityEngine;

using System.Collections.Generic;
using System.Linq;

using Unity.Mathematics;

using Random = UnityEngine.Random;

public class PopulationManager : MonoBehaviour
{
    public GridManager gridManager;

    public int TotalTurns = 20;
    public int IterationCount = 1;
    public int GridWidth = 100;
    public int GridHeight = 100;

    public GameObject agent1Prefab;
    public GameObject agent2Prefab;

    [HideInInspector] public AgentConfiguration agent1 = new AgentConfiguration();
    [HideInInspector] public AgentConfiguration agent2 = new AgentConfiguration();

    GeneticAlgorithm genAlgAgent1;
    GeneticAlgorithm genAlgAgent2;

    List<Agent> populationGOs = new List<Agent>();
    List<Agent> populationGOs1 = new List<Agent>();
    List<Agent> populationGOs2 = new List<Agent>();
    List<Genome> population1 = new List<Genome>();
    List<Genome> population2 = new List<Genome>();
    List<NeuralNetwork> brains1 = new List<NeuralNetwork>();
    List<NeuralNetwork> brains2 = new List<NeuralNetwork>();

    float accumTime = 0;
    bool isRunning = false;

    public int generation { get; private set; }

    public int turns { get; private set; }

    public float bestFitness { get; private set; }

    public float avgFitness { get; private set; }

    public float worstFitness { get; private set; }

    private float getBestFitness()
    {
        float fitness1 = 0;
        foreach (Genome g in population1)
        {
            if (fitness1 < g.fitness)
                fitness1 = g.fitness;
        }

        float fitness2 = 0;
        foreach (Genome g in population2)
        {
            if (fitness2 < g.fitness)
                fitness2 = g.fitness;
        }

        return fitness1 > fitness2 ? fitness1 : fitness2;
    }

    private float getAvgFitness()
    {
        float fitness = 0;
        foreach (Genome g in population1)
        {
            fitness += g.fitness;
        }

        foreach (Genome g in population2)
        {
            fitness += g.fitness;
        }

        return fitness / population1.Count + population2.Count;
    }

    private float getWorstFitness()
    {
        float fitness1 = float.MaxValue;
        foreach (Genome g in population1)
        {
            if (fitness1 > g.fitness)
                fitness1 = g.fitness;
        }

        float fitness2 = float.MaxValue;
        foreach (Genome g in population2)
        {
            if (fitness2 > g.fitness)
                fitness2 = g.fitness;
        }

        return fitness1 > fitness2 ? fitness2 : fitness1;
    }

    static PopulationManager instance = null;

    public static PopulationManager Instance
    {
        get
        {
            if (instance == null)
                instance = FindObjectOfType<PopulationManager>();

            return instance;
        }
    }

    void Awake()
    {
        instance = this;
        Load();
    }

    void Start()
    {
    }

    public void StartSimulation()
    {
        Save();
        // Create and confiugre the Genetic Algorithm
        genAlgAgent1 = new GeneticAlgorithm(agent1.EliteCount, agent1.MutationChance, agent1.MutationRate);
        genAlgAgent2 = new GeneticAlgorithm(agent2.EliteCount, agent2.MutationChance, agent2.MutationRate);

        gridManager.CreateGrid(GridHeight, GridWidth);
        gridManager.CreateFood(agent1.PopulationCount + agent2.PopulationCount, GridHeight, GridWidth);

        GenerateInitialPopulation();

        isRunning = true;
    }

    public void PauseSimulation()
    {
        isRunning = !isRunning;
    }

    public void StopSimulation()
    {
        isRunning = false;

        generation = 0;
        turns = 0;

        DestroyAgents();
    }

    // Generate the random initial population
    void GenerateInitialPopulation()
    {
        generation = 0;
        turns = 0;

        // Destroy previous agents (if there are any)
        DestroyAgents();

        for (int i = 0; i < agent1.PopulationCount; i++)
        {
            NeuralNetwork brain = CreateBrain(1);

            Genome genome = new Genome(brain.GetTotalWeightsCount());

            brain.SetWeights(genome.genome);
            brains1.Add(brain);

            population1.Add(genome);
            populationGOs1.Add(CreateAgent(genome, brain, agent1Prefab, 1, i));
            populationGOs.Add(populationGOs1[i]);
        }

        for (int i = 0; i < agent2.PopulationCount; i++)
        {
            NeuralNetwork brain = CreateBrain(2);

            Genome genome = new Genome(brain.GetTotalWeightsCount());

            brain.SetWeights(genome.genome);
            brains2.Add(brain);

            population2.Add(genome);
            populationGOs2.Add(CreateAgent(genome, brain, agent2Prefab, 2, i));
            populationGOs.Add(populationGOs2[i]);
        }

        accumTime = 0.0f;
    }

    // Creates a new NeuralNetwork
    NeuralNetwork CreateBrain(int agent)
    {
        NeuralNetwork brain = new NeuralNetwork();

        if (agent == 1)
        {
            // Add first neuron layer that has as many neurons as inputs
            brain.AddFirstNeuronLayer(agent1.InputsCount, agent1.Bias, agent1.P);

            for (int i = 0; i < agent1.HiddenLayers; i++)
            {
                // Add each hidden layer with custom neurons count
                brain.AddNeuronLayer(agent1.NeuronsCountPerHL, agent1.Bias, agent1.P);
            }

            // Add the output layer with as many neurons as outputs
            brain.AddNeuronLayer(agent1.OutputsCount, agent1.Bias, agent1.P);
        }
        else if (agent == 2)
        {
            // Add first neuron layer that has as many neurons as inputs
            brain.AddFirstNeuronLayer(agent2.InputsCount, agent2.Bias, agent2.P);

            for (int i = 0; i < agent2.HiddenLayers; i++)
            {
                // Add each hidden layer with custom neurons count
                brain.AddNeuronLayer(agent2.NeuronsCountPerHL, agent2.Bias, agent2.P);
            }

            // Add the output layer with as many neurons as outputs
            brain.AddNeuronLayer(agent2.OutputsCount, agent2.Bias, agent2.P);
        }

        return brain;
    }

    // Evolve!!!
    void Epoch()
    {
        turns = 0;
        // Increment generation counter
        generation++;

        // Calculate best, average and worst fitness
        bestFitness = getBestFitness();
        avgFitness = getAvgFitness();
        worstFitness = getWorstFitness();
        
        for (int i = 0; i < population1.Count; i++)
        {
            if (population1[i].fitness == 0)
            {
                Debug.Log("Killed agent1");
                populationGOs.Remove(populationGOs1[i]);
                Destroy(populationGOs1[i].gameObject);
                populationGOs1.RemoveAt(i);
                population1.RemoveAt(i);
                i--;
            }
        }
        
        for (int i = 0; i < population2.Count; i++)
        {
            if (population2[i].fitness == 0)
            {
                Debug.Log("Killed agent2");
                populationGOs.Remove(populationGOs2[i]);
                Destroy(populationGOs2[i].gameObject);
                populationGOs2.RemoveAt(i);
                population2.RemoveAt(i);
                i--;
            }
        }

        foreach (var agent in populationGOs)
        {
            agent.age++;
            Debug.Log("New age " + agent.age);
        }

        if (populationGOs1.Count > 0&& populationGOs1[0] != null)
        {
            for (int i = 0; i < populationGOs1.Count; i++)
            {
                if (populationGOs1[i].age > 3)
                {
                    Debug.Log("agent1 died of age");
                    populationGOs.Remove(populationGOs1[i]);
                    Destroy(populationGOs1[i].gameObject);
                    populationGOs1.RemoveAt(i);
                    if (i > population1.Count - 1)
                    {
                        Debug.Log("RIP");
                    }

                    population1.RemoveAt(i);
                    i--;
                }
            }
        }

        if (populationGOs2.Count > 0 && populationGOs2[0] != null) 
        {
            for (int i = 0; i < populationGOs2.Count; i++)
            {
                if (populationGOs2[i].age > 3)
                {
                    Debug.Log("agent2 died of age");
                    populationGOs.Remove(populationGOs2[i]);
                    Destroy(populationGOs2[i].gameObject);
                    populationGOs2.RemoveAt(i);
                    if (i > population2.Count - 1)
                    {
                        Debug.Log("RIP");
                    }
                    population2.RemoveAt(i);
                    i--;
                }
            }
        }
        
        
        int agents1ToBreed = population1.Count(a => a.fitness >= 2);
        Debug.Log("Team 1 able to breed count " + agents1ToBreed);
        
        int agents2ToBreed = population2.Count(a => a.fitness >= 2);
        Debug.Log("Team 2 able to breed count " + agents2ToBreed);

        Genome[] newGenomes1 = null;
        Genome[] newGenomes2 = null;
        if (agents1ToBreed >= 2)
        {
            List<Genome> agentsToBreed1 = population1.Where(a => a.fitness >= 2).ToList();
            newGenomes1 = genAlgAgent1.Epoch(agentsToBreed1.ToArray());
        }
        
        if (agents2ToBreed >= 2)
        {
            List<Genome> agentsToBreed2 = population2.Where(a => a.fitness >= 2).ToList();
            newGenomes2 = genAlgAgent2.Epoch(agentsToBreed2.ToArray());
        }

        if (populationGOs1.Count == 0)
        {
            Debug.Log("Agent1 EXTINCT");

            GeneticAlgorithm temp = genAlgAgent2;
            temp.mutationRate *= 2;

            Genome[] newGenomes = temp.Epoch(population2.ToArray());
            population1.Clear();
            brains1.Clear();
            population2.Clear();
            population1.AddRange(newGenomes);

            for (int i = 0; i < newGenomes.Length; i++)
            {
                NeuralNetwork brain = CreateBrain(2);

                Genome genome = newGenomes[i];

                brain.SetWeights(genome.genome);
                brains1.Add(brain);

                populationGOs1.Add(CreateAgent(genome, brain, agent1Prefab, 1, i));
                populationGOs.Add(populationGOs1[i]);
            }
        }
        else if (populationGOs2.Count == 0)
        {
            Debug.Log("Agent2 EXTINCT");

            GeneticAlgorithm temp = genAlgAgent1;
            temp.mutationRate *= 2;

            Genome[] newGenomes = temp.Epoch(population1.ToArray());
            population2.Clear();
            brains2.Clear();
            population2.Clear();
            population2.AddRange(newGenomes);

            for (int i = 0; i < newGenomes.Length; i++)
            {
                NeuralNetwork brain = CreateBrain(1);
                Genome genome = newGenomes[i];

                brain.SetWeights(genome.genome);
                brains2.Add(brain);
                
                populationGOs2.Add(CreateAgent(genome, brain, agent2Prefab, 2, i));
                populationGOs.Add(populationGOs2[i]);
            }
        }
        
        for (int i = 0; i < populationGOs1.Count; i++)
        {
            ResetPos(populationGOs1[i].transform, 1, i);
        }
        
        for (int i = 0; i < populationGOs2.Count; i++)
        {
            ResetPos(populationGOs2[i].transform, 2, i);
        }

        if (newGenomes1 != null)
        {
            int nextXPos = population1.Count - 1;
            population1.AddRange(newGenomes1);
            
            for (int i = 0; i < newGenomes1.Length; i++)
            {
                NeuralNetwork brain = CreateBrain(1);
                Genome genome = newGenomes1[i];

                brain.SetWeights(genome.genome);
                brains1.Add(brain);
                
                Agent newAgent = CreateAgent(agent1Prefab, 1, nextXPos);
                newAgent.SetBrain(genome, brain);
                
                nextXPos++;
                populationGOs1.Add(newAgent);
                populationGOs.Add(newAgent);
            }
            
            agent1.PopulationCount = population1.Count;
        }

        if (newGenomes2 != null)
        {
            int nextXPos = population2.Count - 1;
            population2.AddRange(newGenomes2);
            
            for (int i = 0; i < newGenomes2.Length; i++)
            {
                NeuralNetwork brain = CreateBrain(2);
                Genome genome = newGenomes2[i];

                brain.SetWeights(genome.genome);
                brains2.Add(brain);
                
                Agent newAgent = CreateAgent(agent2Prefab, 2, nextXPos);
                newAgent.SetBrain(genome, brain);

                nextXPos++;
                populationGOs2.Add(newAgent);
                populationGOs.Add(newAgent);
            }
            
            agent2.PopulationCount = population2.Count;
        }

        for (int i = 0; i < populationGOs1.Count; i++)
        {
            populationGOs1[i].Reset();
        }
        for (int i = 0; i < populationGOs2.Count; i++)
        {
            populationGOs2[i].Reset();
        }

        gridManager.ReArrangeFood();
    }

    void ProcessTurn()
    {
        turns++;
        if (turns >= TotalTurns)
        {
            Epoch();
            return;
        }

        foreach (var agent in populationGOs)
        {
            if (agent.dead) continue;

            GameObject nearestFood = GetNearestFood(agent.transform.position);
            agent.SetNearFood(nearestFood);
            
            if (IsOnFood(agent.transform.position))
            {
                agent.isOnFood = true;
                if (IsEnemyOnSameFood(agent.transform.position, agent.isAgent1))
                {
                    agent.isEnemyOnFood = true;
                }
            }

            agent.Think();

            if (IsOnFood(agent.transform.position))
            {
                if (IsEnemyOnSameFood(agent.transform.position, agent.isAgent1))
                {
                    Agent enemy = GetEnemyOnSameFood(agent.transform.position, agent.isAgent1);
                    
                    if (Random.Range(0.0f, 1.0f) > 0.5f)
                    {
                        Debug.Log("Enemy died");
                        enemy.dead = true;

                        agent.EatFood();
                        agent.isOnFood = false;
                        agent.isEnemyOnFood = false;
                        RemoveFood(agent.transform.position);
                    }
                    else
                    {
                        Debug.Log("Agent died");
                        agent.dead = true;

                        enemy.EatFood();
                        enemy.isOnFood = false;
                        enemy.isEnemyOnFood = false;
                        RemoveFood(enemy.transform.position);
                    }
                }
                else
                {
                    Debug.Log("Ate food with no enemies");
                    agent.EatFood();
                    agent.isOnFood = false;
                    agent.isEnemyOnFood = false;
                    RemoveFood(agent.transform.position);
                }
            }

            KeepAgentInBounds(agent);
        }

        for (int i = 0; i < populationGOs1.Count; i++)
        {
            if (populationGOs1[i].dead)
            {
                populationGOs.Remove(populationGOs1[i]);
                Destroy(populationGOs1[i].gameObject);
                populationGOs1.RemoveAt(i);
                i--;
            }
        }

        for (int i = 0; i < populationGOs2.Count; i++)
        {
            if (populationGOs2[i].dead)
            {
                populationGOs.Remove(populationGOs2[i]);
                Destroy(populationGOs2[i].gameObject);
                populationGOs2.RemoveAt(i);
                i--;
            }
        }

        // foreach (var agent1 in  populationGOs1)
        // {
        //     agent1.Think();
        //
        //     Vector3 pos = agent1.transform.position;
        //     if (pos.x >= GridWidth)
        //     {
        //         pos.x = 0;
        //     }
        //     else if (pos.x < 0)
        //     {
        //         pos.x = GridWidth - 1;
        //     }
        //
        //     if (pos.y >= GridHeight)
        //     {
        //         pos.y--;
        //     }
        //     else if (pos.y < 0)
        //     {
        //         pos.y = 0;
        //     }
        //
        //     agent1.transform.position = pos;
        // }
        //
        // foreach (var agent2 in  populationGOs2)
        // {
        //     agent2.Think();
        //     
        //     Vector3 pos = agent2.transform.position;
        //     if (pos.x >= GridWidth)
        //     {
        //         pos.x = 0;
        //     }
        //     else if (pos.x < 0)
        //     {
        //         pos.x = GridWidth - 1;
        //     }
        //
        //     if (pos.y >= GridHeight)
        //     {
        //         pos.y--;
        //     }
        //     else if (pos.y < 0)
        //     {
        //         pos.y = 0;
        //     }
        //
        //     agent2.transform.position = pos;
        // }
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (!isRunning)
            return;

        float dt = Time.fixedDeltaTime;

        for (int i = 0; i < Mathf.Clamp((float)(IterationCount / 100.0f) * 50, 1, 50); i++)
        {
            // Check the time to evolve
            accumTime += dt;
            if (accumTime >= 1.0f)
            {
                accumTime -= 1.0f;
                ProcessTurn();
                break;
            }
        }
    }

    #region Helpers
    Agent CreateAgent(Genome genome, NeuralNetwork brain, GameObject agentPrefab, int agent, int x)
    {
        Vector3 pos = new Vector3(x, 0, 0);
        if (agent == 2)
        {
            pos.y = GridHeight - 1;
        }

        GameObject go = Instantiate(agentPrefab, pos, Quaternion.identity);
        Agent t = go.GetComponent<Agent>();
        t.SetBrain(genome, brain);
        t.isAgent1 = agent == 1;
        GameObject nearestFood = GetNearestFood(pos);
        t.SetNearFood(nearestFood);
        return t;
    }

    Agent CreateAgent(GameObject agentPrefab, int agent, int x)
    {
        Vector3 pos = new Vector3(x, 0, 0);
        if (agent == 2)
        {
            pos.y = GridHeight - 1;
        }
        GameObject go = Instantiate(agentPrefab, pos, Quaternion.identity);
        Agent t = go.GetComponent<Agent>();
        t.isAgent1 = agent == 1;
        return t;
    }

    void DestroyAgents()
    {
        foreach (Agent go in populationGOs1)
            Destroy(go.gameObject);

        foreach (Agent go in populationGOs2)
            Destroy(go.gameObject);

        populationGOs1.Clear();
        population1.Clear();
        brains1.Clear();

        populationGOs2.Clear();
        population2.Clear();
        brains2.Clear();
    }

    GameObject GetNearestFood(Vector3 pos)
    {
        GameObject nearest = gridManager.foods[0];
        float distance = (pos - nearest.transform.position).sqrMagnitude;

        foreach (GameObject go in gridManager.foods)
        {
            float newDist = (go.transform.position - pos).sqrMagnitude;
            if (newDist < distance)
            {
                nearest = go;
                distance = newDist;
            }
        }

        return nearest;
    }

    bool IsOnFood(Vector3 pos)
    {
        return gridManager.foods.Exists(food => food.transform.position == pos);
    }

    bool IsEnemyOnSameFood(Vector3 pos, bool isAgent1)
    {
        if (isAgent1)
        {
            return populationGOs2.Exists(agent => agent.transform.position == pos);
        }
        else
        {
            return populationGOs1.Exists(agent => agent.transform.position == pos);
        }
    }

    Agent GetEnemyOnSameFood(Vector3 pos, bool isAgent1)
    {
        if (isAgent1)
        {
            return populationGOs2.Find(agent => agent.transform.position == pos);
        }
        else
        {
            return populationGOs1.Find(agent => agent.transform.position == pos);
        }
    }

    void KeepAgentInBounds(Agent agent)
    {
        Vector3 pos = agent.transform.position;
        if (pos.x >= GridWidth)
        {
            pos.x = 0;
        }
        else if (pos.x < 0)
        {
            pos.x = GridWidth - 1;
        }

        if (pos.y >= GridHeight)
        {
            pos.y--;
        }
        else if (pos.y < 0)
        {
            pos.y = 0;
        }

        agent.transform.position = pos;
    }

    void RemoveFood(Vector3 pos)
    {
        int index = gridManager.foods.FindIndex(food => food.transform.position == pos);

        Destroy(gridManager.foods[index].gameObject);
        gridManager.foods.RemoveAt(index);
    }

    void ResetPos(Transform agentPos, int agent, int x)
    {
        Vector3 pos = new Vector3(x, 0, 0);
        if (agent == 2)
        {
            pos.y = GridHeight - 1;
        }

        agentPos.position = pos;
    }
    
    public void Load()
    {
        agent1.PopulationCount = PlayerPrefs.GetInt("PopulationCount", 2);
        agent1.EliteCount = PlayerPrefs.GetInt("EliteCount", 0);
        agent1.MutationChance = PlayerPrefs.GetFloat("MutationChance", 0);
        agent1.MutationRate = PlayerPrefs.GetFloat("MutationRate", 0);
        agent1.InputsCount = PlayerPrefs.GetInt("InputsCount", 1);
        agent1.HiddenLayers = PlayerPrefs.GetInt("HiddenLayers", 5);
        agent1.OutputsCount = PlayerPrefs.GetInt("OutputsCount", 1);
        agent1.NeuronsCountPerHL = PlayerPrefs.GetInt("NeuronsCountPerHL", 1);
        agent1.Bias = PlayerPrefs.GetFloat("Bias", 0);
        agent1.P = PlayerPrefs.GetFloat("P", 1);
        
        agent2.PopulationCount = PlayerPrefs.GetInt("PopulationCount2", 2);
        agent2.EliteCount = PlayerPrefs.GetInt("EliteCount2", 0);
        agent2.MutationChance = PlayerPrefs.GetFloat("MutationChance2", 0);
        agent2.MutationRate = PlayerPrefs.GetFloat("MutationRate2", 0);
        agent2.InputsCount = PlayerPrefs.GetInt("InputsCount2", 1);
        agent2.HiddenLayers = PlayerPrefs.GetInt("HiddenLayers2", 5);
        agent2.OutputsCount = PlayerPrefs.GetInt("OutputsCount2", 1);
        agent2.NeuronsCountPerHL = PlayerPrefs.GetInt("NeuronsCountPerHL2", 1);
        agent2.Bias = PlayerPrefs.GetFloat("Bias2", 0);
        agent2.P = PlayerPrefs.GetFloat("P2", 1);
    }

    void Save()
    {
        PlayerPrefs.SetInt("PopulationCount", agent1.PopulationCount);
        PlayerPrefs.SetInt("EliteCount", agent1.EliteCount);
        PlayerPrefs.SetFloat("MutationChance", agent1.MutationChance);
        PlayerPrefs.SetFloat("MutationRate", agent1.MutationRate);
        PlayerPrefs.SetInt("InputsCount", agent1.InputsCount);
        PlayerPrefs.SetInt("HiddenLayers",agent1. HiddenLayers);
        PlayerPrefs.SetInt("OutputsCount", agent1.OutputsCount);
        PlayerPrefs.SetInt("NeuronsCountPerHL", agent1.NeuronsCountPerHL);
        PlayerPrefs.SetFloat("Bias", agent1.Bias);
        PlayerPrefs.SetFloat("P", agent1.P);
        
        PlayerPrefs.SetInt("PopulationCount2", agent2.PopulationCount);
        PlayerPrefs.SetInt("EliteCount2", agent2.EliteCount);
        PlayerPrefs.SetFloat("MutationChance2", agent2.MutationChance);
        PlayerPrefs.SetFloat("MutationRate2", agent2.MutationRate);
        PlayerPrefs.SetInt("InputsCount2", agent2.InputsCount);
        PlayerPrefs.SetInt("HiddenLayers2",agent2. HiddenLayers);
        PlayerPrefs.SetInt("OutputsCount2", agent2.OutputsCount);
        PlayerPrefs.SetInt("NeuronsCountPerHL2", agent2.NeuronsCountPerHL);
        PlayerPrefs.SetFloat("Bias2", agent2.Bias);
        PlayerPrefs.SetFloat("P2", agent2.P);
    }
    #endregion
}