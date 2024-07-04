using System;

[Flags]
public enum PlayerPhysicsInteractionType
{
	None = 0,

	InteractTriggerEnter2D = 1 << 0,

	InteractTriggerExit2D = 1 << 1,

	All = ~(-1 << 2)
}