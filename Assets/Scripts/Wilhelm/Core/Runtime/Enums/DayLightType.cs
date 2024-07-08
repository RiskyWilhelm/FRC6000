using System;

[Flags]
public enum DaylightType
{
	None = 0,

	Night = 1 << 0,

	Light = 1 << 1,

	All = ~(-1 << 2)
}