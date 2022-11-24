using System;

using UnityEngine;

using Vector3 = UnityEngine.Vector3;

[Serializable]
public class AgentBase : MonoBehaviour
{
    public Genome genome;
    public NeuralNetwork brain;
    protected float[] inputs;
    protected GameObject nearFood;
    public Vector3 lastPos;

    public bool isOnFood = false;
    public bool isOnCellWithEnemy = false;
    public bool isOnCellWithAlly = false;
    public bool isAgent1 = false;
    public bool dead = false;
    public bool ranAway = false;

    public void SetBrain(Genome genome, NeuralNetwork brain)
    {
        this.genome = genome;
        this.brain = brain;
        inputs = new float[brain.InputsCount];
        OnReset();
    }

    public void Reset()
    {
        OnReset();
    }

    public void SetNearFood(GameObject nearFood)
    {
        this.nearFood = nearFood;
    }

    protected void Move(float sameYasFood, float foodX)
    {
        lastPos = transform.position;

        bool moveUp = sameYasFood > 0.75f;
        float positive = foodX < 0.5f ? -1f : 1f;
        
        Vector3 movement = new Vector3(!moveUp ? positive : 0f, moveUp ? positive : 0f, 0f);
        transform.Translate(movement);
        
        // if (foodX > foodY+0.5f)
        // {
        //     switch (foodX)
        //     {
        //         case < 0.5f: //Left
        //             transform.Translate(Vector3.left);
        //             Debug.Log("Left");
        //             break;
        //         case >= 0.5f: //Right
        //             transform.Translate(Vector3.right);
        //             Debug.Log("Right");
        //             break;
        //     }
        // }
        // else
        // {
        //     switch (foodY)
        //     {
        //         case < 0.5f: //Up
        //             transform.Translate(Vector3.up);
        //             Debug.Log("Up");
        //             break;
        //         case >= 0.5f: //Down
        //             transform.Translate(Vector3.down);
        //             Debug.Log("Down");
        //             break;
        //     }
        // }
        
        // if (foodX > foodY)
        // {
        //     switch (xDir)
        //     {
        //         case < 0.5f: //Left
        //             transform.Translate(Vector3.left);
        //             Debug.Log("Left");
        //             break;
        //         case >= 0.5f: //Right
        //             transform.Translate(Vector3.right);
        //             Debug.Log("Right");
        //             break;
        //     }
        // }
        // else
        // {
        //     switch (yDir)
        //     {
        //         case < 0.5f: //Up
        //             transform.Translate(Vector3.up);
        //             Debug.Log("Up");
        //             break;
        //         case >= 0.5f: //Down
        //             transform.Translate(Vector3.down);
        //             Debug.Log("Down");
        //             break;
        //     }
        // }
    }

    protected void FightOrFlight(float stay)
    {
        if (stay > 0.5f)
        {
            Debug.Log("Stays on food");
        }
        else
        {
            Retreat();
        }
    }
    
    protected Vector3 GetDirToFood(Vector3 food)
    {
        return (food - transform.position).normalized;
    }

    public void Retreat()
    {
        ranAway = true;
        Debug.Log("Retreated to previous pos");
        (transform.position, lastPos) = (lastPos, transform.position);
    }
    
    public void Think()
    {
        OnThink();
    }

    public void EatFood()
    {
        OnEatFood();
    }

    protected virtual void OnEatFood()
    {
    }

    protected virtual void OnThink()
    {
    }

    protected virtual void OnReset()
    {
    }
}