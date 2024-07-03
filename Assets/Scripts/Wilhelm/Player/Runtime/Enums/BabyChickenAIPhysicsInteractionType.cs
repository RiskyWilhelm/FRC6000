using System;

[Flags]
public enum BabyChickenAIPhysicsInteractionType
{
	None = 0,

	EnemyTriggerStay2D = 1 << 0,

	All = ~(-1 << 1)
}