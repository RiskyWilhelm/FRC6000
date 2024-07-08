// WARNING: If any changes be made, one must update the "Luck" class
using System;

[Flags]
public enum LuckType
{
	None = 0,

	VeryCommon = 1 << 0,

	Common = 1 << 1,

	UnCommon = 1 << 2,

	Rare = 1 << 4,

	VeryRare = 1 << 5,

	Impossible = 1 << 8,

	All = ~(-1 << 9)
}