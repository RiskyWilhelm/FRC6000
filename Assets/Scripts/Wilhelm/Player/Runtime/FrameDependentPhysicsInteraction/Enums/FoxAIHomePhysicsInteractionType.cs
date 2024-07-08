using System;

[Flags]
public enum FoxAIHomePhysicsInteractionType
{
	None = 0,

	GateTriggerStay2D = 1 << 0,

	StealBabyChickenTriggerEnter2D = 1 << 1,

	All = ~(-1 << 2)
}