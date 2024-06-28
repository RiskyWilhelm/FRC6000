using System;

[Flags]
public enum WarrirorChickenAITriggerType
{
	None = 0,

	SingleNormalAttackTriggerStay = 1 << 0,

	SingleNormalAttackTriggerExit = 1 << 1,

	EnemyTriggerStay = 1 << 2,

	EnemyTriggerExit = 1 << 3,

	All = ~(-1 << 4)
}