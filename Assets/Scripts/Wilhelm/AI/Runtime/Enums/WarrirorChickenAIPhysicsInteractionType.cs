using System;

[Flags]
public enum WarrirorChickenAIPhysicsInteractionType
{
	None = 0,

	SingleNormalAttackTriggerStay2D = 1 << 0,

	SingleNormalAttackTriggerExit2D = 1 << 1,

	EnemyTriggerStay2D = 1 << 2,

	EnemyTriggerExit2D = 1 << 3,

	All = ~(-1 << 4)
}