// WARNING: If any changes be made, one must update the "Luck" class
// 011111111
using System;

[Flags]
public enum LuckType
{
	None = 0,
	VeryCommon = 1 << 0, // 1
	Common = 1 << 1, // 2
	UnCommon = 1 << 2, // 4
	Rare = 1 << 4, // 16
	VeryRare = 1 << 5, // 32
	Impossible = 1 << 8, // 256
}