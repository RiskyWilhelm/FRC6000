using System;

public static class EnumUtils
{
	/// <summary> Throws exception on mismatch </summary>
	public static void ValidateUnderlyingType<EnumType, EnumUnderlyingType>()
		where EnumType : Enum
		where EnumUnderlyingType : struct
	{
		if (Enum.GetUnderlyingType(typeof(EnumType)) != typeof(EnumUnderlyingType))
			throw new NotSupportedException("Underlying value Type of EnumType and EnumUnderlyingType must be same and enums only support; byte, sbyte, short, ushort, int, uint, long, or ulong");
	}

	public static void ValidateFlagEnum<T>()
		where T : Enum
	{
		if (!typeof(T).IsDefined(typeof(FlagsAttribute), inherit: false))
			throw new NotSupportedException("Only flag enums with Flags attribute supported");
	}
}
