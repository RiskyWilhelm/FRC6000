using System;

[Flags]
public enum ChickenAIHomePhysicsInteractionType
{
	None = 0,

	GateTriggerStay2D = 1 << 0,

	All = ~(-1 << 1)
}