using UnityEngine;

public class Agent : AgentBase
{
    private float fitness = 0;
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
        Vector3 foodPosition = nearFood.transform.position;

        if (inputs == null)
        {
            Debug.Log("NULL INPUTs");
        }

        if (transform == null)
        {
            Debug.Log("NULL trans");
        }

        inputs[0] = position.x;
        inputs[1] = position.y;
        inputs[2] = foodPosition.x;
        inputs[3] = foodPosition.y;
        inputs[4] = isOnFood ? 1.0f : -1.0f;
        inputs[5] = isOnFood ? 1.0f : -1.0f;
        inputs[6] = isEnemyOnFood ? 1.0f : -1.0f;

        float[] outputs = brain.Synapsis(inputs);

        if (outputs[5] > 0.5f && outputs[6] > 0.25f) 
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