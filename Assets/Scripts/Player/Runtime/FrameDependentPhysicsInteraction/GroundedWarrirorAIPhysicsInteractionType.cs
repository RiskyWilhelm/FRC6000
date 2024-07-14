using System;

[Flags]
public enum GroundedWarrirorAIPhysicsInteractionType
{
	None = 0,

	SingleNormalAttackTriggerStay2D = 1 << 0,

	SingleNormalAttackTriggerExit2D = 1 << 1,

	EnemyTriggerEnter2D = 1 << 2,

	EnemyTriggerStay2D = 1 << 3,

	EnemyTriggerExit2D = 1 << 4,

	All = ~(-1 << 5)
}