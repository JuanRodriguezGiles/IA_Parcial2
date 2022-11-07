public class Agent : AgentBase
{
    float fitness = 0;
    
    protected override void OnReset()
    {
        fitness = 1;
    }

    protected override void OnThink(float dt) 
	{
        
	}
}