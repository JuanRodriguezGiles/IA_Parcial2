using System;

using UnityEngine;

using Vector3 = UnityEngine.Vector3;

[Serializable]
public class AgentBase : MonoBehaviour
{
    public Genome genome;
    protected NeuralNetwork brain;
    protected float[] inputs;
    protected GameObject nearFood;
    protected Vector3 lastPos;

    public bool isOnFood = false;
    public bool isEnemyOnFood = false;
    public bool isAgent1 = false;
    public bool dead = false;

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

    protected void Move(float x, float y, float xDir, float yDir)
    {
        if (isOnFood && isEnemyOnFood)
        {
            Debug.Log("Retreated to previous pos");
            (transform.position, lastPos) = (lastPos, transform.position);
        }
        else
        {
            lastPos = transform.position;

            if (x > y)
            {
                switch (xDir)
                {
                    case < 0.5f: //Left
                        transform.Translate(Vector3.left);
                        Debug.Log("Left");
                        break;
                    case >= 0.5f: //Right
                        transform.Translate(Vector3.right);
                        Debug.Log("Right");
                        break;
                }
            }
            else
            {
                switch (yDir)
                {
                    case < 0.5f: //Up
                        transform.Translate(Vector3.up);
                        Debug.Log("Up");
                        break;
                    case >= 0.5f: //Down
                        transform.Translate(Vector3.down);
                        Debug.Log("Down");
                        break;
                }
            }
        }
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