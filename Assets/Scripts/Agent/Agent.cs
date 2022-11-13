using System;

using UnityEngine;

[Serializable]
public class Agent : AgentBase
{
    public float fitness = 0;
    public int age = 1;

    protected override void OnReset()
    {
        fitness = 0;
        isOnFood = false;
        isEnemyOnFood = false;
        dead = false;
    }

    protected override void OnThink()
    {
        Vector3 position = transform.position;
        Vector3 foodDir = GetDirToFood(nearFood);

        inputs[0] = position.x;
        inputs[1] = position.y;
        inputs[2] = foodDir.x;
        inputs[3] = foodDir.y;
        inputs[4] = isOnFood ? 1.0f : -1.0f;
        inputs[5] = isOnFood ? 1.0f : -1.0f;
        inputs[6] = isEnemyOnFood ? 1.0f : -1.0f;

        float[] outputs = brain.Synapsis(inputs);

        if (isOnFood)
        {
            FightOrFlight(outputs[6]);
        }
        else if (outputs[4] > 0.5f) 
        {
            Move(outputs[0], outputs[1], outputs[2], outputs[3]);
        }
        else
        {
            Debug.Log("No move");
        }
    }

    protected override void OnEatFood()
    {
        fitness++;
        genome.fitness = fitness;
    }
}