using UnityEngine;

public abstract partial class AIPreReadyStatsSOBase : ScriptableObject, ICopyable<AIStatsBase>
{
	public abstract void Copy(in AIStatsBase other);

	public abstract void CopyTo(in AIStatsBase main);
}

public abstract partial class AIPreReadyStatsSOBase<AIStatsType> : AIPreReadyStatsSOBase
	where AIStatsType : AIStatsBase
{
	[SerializeField]
	private AIStatsType preReadyStats;

	public override void Copy(in AIStatsBase other)
	{
		other.CopyTo(this.preReadyStats);
	}

	public override void CopyTo(in AIStatsBase main)
	{
		preReadyStats.CopyTo(main);
	}
}


#if UNITY_EDITOR

public abstract partial class AIPreReadyStatsSOBase
{ }

#endif