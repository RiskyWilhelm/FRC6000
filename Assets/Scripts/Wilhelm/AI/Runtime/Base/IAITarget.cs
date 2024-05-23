using UnityEngine;

public interface IAITarget
{
	public Vector2 Position { get; }

	public float OthersMaxApproachDistance { get; }

	public byte Power { get; }


	/// <summary> Called when other AI caught self this </summary>
	public void OnGotCaughtBy(AIBase chaser);

	public bool IsChaseableBy(IAITarget otherAI) => Power <= otherAI.Power;
}