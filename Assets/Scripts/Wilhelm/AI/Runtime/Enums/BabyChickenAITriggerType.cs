using System;

[Flags]
public enum BabyChickenAITriggerType
{
	None = 0,

	RunawayTriggerStay = 1 << 0,

	All = ~(-1 << 1)
}