using UnityEngine;

using System.Collections.Generic;
using System.IO;
using System.Linq;

using Random = UnityEngine.Random;

public class PopulationManager : MonoBehaviour
{
    public GridManager gridManager;

    public int TotalTurns = 20;
    public int IterationCount = 1;
    public int GridWidth = 100;
    public int GridHeight = 100;

    private int lastSavedGenome1 = 0;
    private int lastSavedGenome2 = 0;

    public GameObject agent1Prefab;
    public GameObject agent2Prefab;

    public bool useSavedGenomes;
    public bool resetLoadCount;

    [HideInInspector] public AgentConfiguration agent1 = new AgentConfiguration();
    [HideInInspector] public AgentConfiguration agent2 = new AgentConfiguration();

    GeneticAlgorithm genAlgAgent1;
    GeneticAlgorithm genAlgAgent2;

    public List<Agent> populationGOs = new List<Agent>();
    public List<Agent> populationGOs1 = new List<Agent>();
    public List<Agent> populationGOs2 = new List<Agent>();
    public List<Genome> population1 = new List<Genome>();
    public List<Genome> population2 = new List<Genome>();
    List<NeuralNetwork> brains1 = new List<NeuralNetwork>();
    List<NeuralNetwork> brains2 = new List<NeuralNetwork>();

    float accumTime = 0;
    bool isRunning = false;

    private DataModel dataModel1;
    private DataModel dataModel2;

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
        
        if (resetLoadCount)
        {
            useSavedGenomes = false;
            PlayerPrefs.DeleteKey("LastSave1");
            PlayerPrefs.DeleteKey("LastSave2");
        }
        
        if (useSavedGenomes)
        {
            string json1 = File.ReadAllText(Application.persistentDataPath + "/agent1GenomeV" + (lastSavedGenome1 - 1) + ".json");
            dataModel1 = JsonUtility.FromJson<DataModel>(json1);

            string json2 = File.ReadAllText(Application.persistentDataPath + "/agent2GenomeV" + (lastSavedGenome2 - 1) + ".json");
            dataModel2 = JsonUtility.FromJson<DataModel>(json2);
            
            gridManager.CreateFood(dataModel1.genome.Count + dataModel2.genome.Count, GridHeight, GridWidth);
        }
        else
        {
            gridManager.CreateFood(agent1.PopulationCount + agent2.PopulationCount, GridHeight, GridHeight);
        }
        
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

        if (useSavedGenomes)
        {
            string json1 = File.ReadAllText(Application.persistentDataPath + "/agent1GenomeV" + lastSavedGenome1 + ".json");
            DataModel dataModel1 = JsonUtility.FromJson<DataModel>(json1);

            string json2 = File.ReadAllText(Application.persistentDataPath + "/agent2GenomeV" + lastSavedGenome2 + ".json");
            DataModel dataModel2 = JsonUtility.FromJson<DataModel>(json2);

            for (int i = 0; i < dataModel1.genome.Count; i++) 
            {
               brains1.Add(dataModel1.brain[i]);
               population1.Add(dataModel1.genome[i]);
               populationGOs1.Add(CreateAgent(dataModel1.genome[i], dataModel1.brain[i], agent1Prefab, 1, i));
               populationGOs.Add(populationGOs1[i]);
            }
            
            for (int i = 0; i < dataModel2.genome.Count; i++) 
            {
                brains2.Add(dataModel2.brain[i]);
                population2.Add(dataModel2.genome[i]);
                populationGOs2.Add(CreateAgent(dataModel2.genome[i], dataModel2.brain[i], agent2Prefab, 2, i));
                populationGOs.Add(populationGOs2[i]);
            }
        }
        else
        {
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

        if (generation % 2 == 0) 
        {
            SavePopulation();
        }
        
        // Calculate best, average and worst fitness
        bestFitness = getBestFitness();
        avgFitness = getAvgFitness();
        worstFitness = getWorstFitness();

        for (int i = populationGOs.Count - 1; i >= 0; i--)
        {
            if (populationGOs[i].foodEaten == 0 || populationGOs[i].age > 3)
            {
                Debug.Log("Agent " + i + "died");
                Agent agent = populationGOs[i];
                if (agent.isAgent1)
                {
                    populationGOs1.Remove(agent);
                    population1.Remove(agent.genome);
                    brains1.Remove(agent.brain);
                }
                else
                {
                    populationGOs2.Remove(agent);
                    population2.Remove(agent.genome);
                    brains2.Remove(agent.brain);
                }

                populationGOs.Remove(agent);
                Destroy(agent.gameObject);
            }
        }

        foreach (var agent in populationGOs)
        {
            agent.age++;
            Debug.Log("New age " + agent.age);
        }

        if (populationGOs1.Count == 0)
        {
            Debug.Log("Population1 EXTINCT");

            GeneticAlgorithm temp = genAlgAgent2;
            temp.mutationRate *= 2;

            Genome[] newGenomes = temp.Epoch(population2.ToArray());
            population1.Clear();
            brains1.Clear();
            population1.Clear();
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
            Debug.Log("Population2 EXTINCT");

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

        for (int i = 0; i < populationGOs.Count; i++)
        {
            ResetPos(populationGOs[i].transform, populationGOs[i].isAgent1, i);
        }

        int agents1ToBreed = populationGOs1.Count(a => a.foodEaten >= 2);
        Debug.Log("Team 1 able to breed count " + agents1ToBreed);

        int agents2ToBreed = populationGOs2.Count(a => a.foodEaten >= 2);
        Debug.Log("Team 2 able to breed count " + agents2ToBreed);


        if (agents1ToBreed >= 2)
        {
            Breed(true);
        }

        if (agents2ToBreed >= 2)
        {
            Breed(false);
        }

        for (int i = 0; i < populationGOs.Count; i++)
        {
            populationGOs[i].Reset();
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

        //1 Move all agents
        foreach (var agent in populationGOs)
        {
            GameObject nearestFood = GetNearestFood(agent.transform.position);
            agent.SetNearFood(nearestFood);

            agent.Think();
            KeepAgentInBounds(agent);
        }

        //2 Check for agents on food
        foreach (var agent in populationGOs.Where(agent => IsOnFood(agent.transform.position)))
        {
            agent.isOnFood = true;
            if (IsEnemyOnSameFood(agent.transform.position, agent.isAgent1))
            {
                agent.isEnemyOnFood = true;
            }

            agent.Think();
            KeepAgentInBounds(agent);
        }

        //3 Resolve agents still on food
        foreach (var agent in populationGOs.Where(agent => IsOnFood(agent.transform.position)))
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

        //Kill dead agents / reset
        for (int i = 0; i < populationGOs.Count; i++)
        {
            if (populationGOs[i].dead)
            {
                Agent agent = populationGOs[i];
                if (agent.isAgent1)
                {
                    populationGOs1.Remove(agent);
                    population1.Remove(agent.genome);
                    brains1.Remove(agent.brain);
                }
                else
                {
                    populationGOs2.Remove(agent);
                    population2.Remove(agent.genome);
                    brains2.Remove(agent.brain);
                }

                populationGOs.Remove(agent);
                Destroy(agent.gameObject);
            }
            else
            {
                populationGOs[i].isOnFood = false;
                populationGOs[i].isEnemyOnFood = false;
            }
        }
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
        return isAgent1 ? populationGOs2.Find(agent => agent.transform.position == pos) : populationGOs1.Find(agent => agent.transform.position == pos);
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

    void ResetPos(Transform agentPos, bool isAgent1, int x)
    {
        Vector3 pos = new Vector3(x, 0, 0);
        if (!isAgent1)
        {
            pos.y = GridHeight - 1;
        }

        agentPos.position = pos;
    }

    private void Load()
    {
        TotalTurns = PlayerPrefs.GetInt("TotalTurns", 100);
        GridHeight = PlayerPrefs.GetInt("GridSize", 100);
        GridWidth = PlayerPrefs.GetInt("GridSize", 100);

        lastSavedGenome1 = PlayerPrefs.GetInt("LastSave1", 0);
        lastSavedGenome2 = PlayerPrefs.GetInt("LastSave2", 0);

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

    private void Save()
    {
        PlayerPrefs.SetInt("TotalTurns", TotalTurns);
        PlayerPrefs.SetInt("GridSize", GridHeight);

        PlayerPrefs.SetInt("PopulationCount", agent1.PopulationCount);
        PlayerPrefs.SetInt("EliteCount", agent1.EliteCount);
        PlayerPrefs.SetFloat("MutationChance", agent1.MutationChance);
        PlayerPrefs.SetFloat("MutationRate", agent1.MutationRate);
        PlayerPrefs.SetInt("InputsCount", agent1.InputsCount);
        PlayerPrefs.SetInt("HiddenLayers", agent1.HiddenLayers);
        PlayerPrefs.SetInt("OutputsCount", agent1.OutputsCount);
        PlayerPrefs.SetInt("NeuronsCountPerHL", agent1.NeuronsCountPerHL);
        PlayerPrefs.SetFloat("Bias", agent1.Bias);
        PlayerPrefs.SetFloat("P", agent1.P);

        PlayerPrefs.SetInt("PopulationCount2", agent2.PopulationCount);
        PlayerPrefs.SetInt("EliteCount2", agent2.EliteCount);
        PlayerPrefs.SetFloat("MutationChance2", agent2.MutationChance);
        PlayerPrefs.SetFloat("MutationRate2", agent2.MutationRate);
        PlayerPrefs.SetInt("InputsCount2", agent2.InputsCount);
        PlayerPrefs.SetInt("HiddenLayers2", agent2.HiddenLayers);
        PlayerPrefs.SetInt("OutputsCount2", agent2.OutputsCount);
        PlayerPrefs.SetInt("NeuronsCountPerHL2", agent2.NeuronsCountPerHL);
        PlayerPrefs.SetFloat("Bias2", agent2.Bias);
        PlayerPrefs.SetFloat("P2", agent2.P);
    }

    private void Breed(bool isAgent1)
    {
        Genome[] newGenomes = null;

        if (isAgent1)
        {
            List<Genome> agentsToBreed = new List<Genome>();

            for (int i = 0; i < populationGOs1.Count; i++)
            {
                if (populationGOs1[i].foodEaten >= 2)
                {
                    agentsToBreed.Add(populationGOs1[i].genome);
                }
            }

            //Make list even
            if (agentsToBreed.Count % 2 != 0)
            {
                agentsToBreed.RemoveAt(agentsToBreed.Count - 1);
            }
            
            newGenomes = genAlgAgent1.Epoch(agentsToBreed.ToArray());

            int nextXPos = population1.Count - 1;
            population1.AddRange(newGenomes);

            for (int i = 0; i < newGenomes.Length; i++)
            {
                NeuralNetwork brain = CreateBrain(1);
                Genome genome = newGenomes[i];

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
        else
        {
            List<Genome> agentsToBreed = new List<Genome>();

            for (int i = 0; i < populationGOs2.Count; i++)
            {
                if (populationGOs2[i].foodEaten >= 2)
                {
                    agentsToBreed.Add(populationGOs2[i].genome);
                }
            }

            newGenomes = genAlgAgent2.Epoch(agentsToBreed.ToArray());

            int nextXPos = population2.Count - 1;
            population2.AddRange(newGenomes);

            for (int i = 0; i < newGenomes.Length; i++)
            {
                NeuralNetwork brain = CreateBrain(2);
                Genome genome = newGenomes[i];

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
    }

    private void SavePopulation()
    {
        Debug.Log("Saved population");
       
            DataModel brainData1 = new DataModel
            {
                genome = population1,
                brain = brains1
            };
            File.WriteAllText(Application.persistentDataPath + "/agent1GenomeV" + lastSavedGenome1 + ".json", JsonUtility.ToJson(brainData1));
            lastSavedGenome1++;
            PlayerPrefs.SetInt("LastSave1", lastSavedGenome1);
      
            DataModel brainData2 = new DataModel
            {
                genome = population2,
                brain = brains2
            };
            File.WriteAllText(Application.persistentDataPath + "/agent2GenomeV" + lastSavedGenome2 + ".json", JsonUtility.ToJson(brainData2));
            lastSavedGenome2++;
            PlayerPrefs.SetInt("LastSave2", lastSavedGenome1);
      
    }
    #endregion
}