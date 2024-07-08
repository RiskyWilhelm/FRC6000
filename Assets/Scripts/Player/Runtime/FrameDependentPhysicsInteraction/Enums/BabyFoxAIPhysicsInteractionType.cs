using System;

[Flags]
public enum BabyFoxAIPhysicsInteractionType
{
	None = 0,

	EnemyTriggerStay2D = 1 << 0,

	All = ~(-1 << 1)
}