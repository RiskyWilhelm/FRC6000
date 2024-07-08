using System;

[Flags]
public enum DayType
{
	None = 0,

	AM = 1 << 0,

	PM = 1 << 1,

	All = ~(-1 << 2)
}