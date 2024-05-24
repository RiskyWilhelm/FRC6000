public sealed partial class ChickenAI : AIBase
{
	public void OnGotCaughtBy(AIBase chaser)
	{
		ChickenAIPool.Release(this);
	}
}


#if UNITY_EDITOR

public sealed partial class ChickenAI
{ }

#endif