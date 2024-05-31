using UnityEngine;

public abstract partial class AIPreReadyStatsSOBase<AIStatsType> : ScriptableObject, ICopyable<AIStatsType>
	where AIStatsType : AIStatsBase, ICopyable<AIStatsType>
{
	[SerializeField]
	private AIStatsType preReadyStats;

	public void CopyTo(in AIStatsType main) => preReadyStats.CopyTo(main);
}


#if UNITY_EDITOR

public abstract partial class AIPreReadyStatsSOBase<AIStatsType>
{ }

#endif