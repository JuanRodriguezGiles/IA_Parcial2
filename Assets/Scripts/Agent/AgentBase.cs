using UnityEngine;

public class AgentBase : MonoBehaviour
{
    protected Genome genome;
    protected NeuralNetwork brain;
    protected float[] inputs;

    public void SetBrain(Genome genome, NeuralNetwork brain)
    {
        this.genome = genome;
        this.brain = brain;
        inputs = new float[brain.InputsCount];
        OnReset();
    }

    public void Think(float dt)
    {
        OnThink(dt);
    }

    protected virtual void OnThink(float dt)
    {
    }

    protected virtual void OnReset()
    {
    }
}