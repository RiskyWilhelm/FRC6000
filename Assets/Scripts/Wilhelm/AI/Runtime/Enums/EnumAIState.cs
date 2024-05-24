public enum AIState
{
	/// <summary> Lowest priority </summary>
	Idle,

	/// <summary> Chasing the target. Acts like Walking or Running </summary>
	Running,

	/// <summary> Attacking to the target </summary>
	Attacking,
}