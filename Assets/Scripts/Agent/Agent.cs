using System;

using UnityEngine;

[Serializable]
public class Agent : AgentBase
{
    public float fitness = 0;
    public float age = 1;
    public float foodEaten = 0;
    
    public Agent enemy;
    
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

        inputs[0] = foodPos.y == position.y ? 1.0f : -1.0f;
        inputs[1] = foodPos.x;
        inputs[2] = foodEaten > 0 ? foodEaten : -1.0f;
        inputs[3] = age;

        float[] outputs = brain.Synapsis(inputs);

        if (isOnFood) 
        {
            FightOrFlight(outputs[3]);
        }
        else if (isOnCellWithEnemy)
        {
            FightOrFlight(outputs[2]);
        }
        else if (outputs[2] > 0.5f)
        {
            Move(outputs[0], outputs[1]);
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