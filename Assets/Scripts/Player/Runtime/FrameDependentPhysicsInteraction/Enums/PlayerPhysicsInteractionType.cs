using System;

[Flags]
public enum PlayerPhysicsInteractionType
{
	None = 0,

	EnemyTriggerEnter2D = 1 << 0,

	EnemyTriggerExit2D = 1 << 1,

	All = ~(-1 << 2)
}