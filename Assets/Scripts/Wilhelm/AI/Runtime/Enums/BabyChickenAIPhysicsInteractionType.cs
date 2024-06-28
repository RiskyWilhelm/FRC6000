using System;

[Flags]
public enum BabyChickenAIPhysicsInteractionType
{
	None = 0,

	RunawayTriggerStay = 1 << 0,

	All = ~(-1 << 1)
}