using System;

using UnityEngine;

[Serializable]
public class Agent : AgentBase
{
    public float fitness = 0;
    public float age = 1;
    public float foodEaten = 0;
    
    public Agent agentOnCell;
    
    protected override void OnReset()
    {
        genome.fitness = 0;
        
        fitness = 0;
        foodEaten = 0;

        isOnFood = false;
        isOnCellWithEnemy = false;
        isOnCellWithAlly = false;
        dead = false;
        ranAway = false;
    }

    protected override void OnThink()
    {
        Vector3 position = transform.position;
        Vector3 foodPos = nearFood.transform.position;
        //Vector3 foodDir = GetDirToFood(foodPos);

        inputs[0] = foodPos.y != position.y ? 1.0f : -1.0f;
        inputs[1] = foodPos.y != position.y ? 1.0f : -1.0f;
        inputs[2] = foodPos.x;
        inputs[3] = foodEaten > 0 ? foodEaten : -1.0f;
        inputs[4] = age;
        inputs[5] = position.y > foodPos.y ? 1.0f : -1.0f;

        float[] outputs = brain.Synapsis(inputs);

        if (isOnFood) 
        {
            FightOrFlight(outputs[4]);
        }
        else if (isOnCellWithEnemy)
        {
            FightOrFlight(outputs[3]);
        }
        else if (outputs[2] > 0.5f)
        {
            Move(outputs[1], outputs[5], outputs[2]);
        }
        else
        {
            Debug.Log("No move");
        }
    }

    protected override void OnEatFood()
    {
        foodEaten++;
        
        if (foodEaten == 2)
        {
            fitness += 10;
        }
        else
        {
            fitness++;
        }
        
        genome.fitness = fitness;
    }
}