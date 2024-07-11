using UnityEngine;

/// <summary> Customizes the field shown in inspector and clamps the values as <see cref="RangeAttribute"/> does </summary>
public sealed partial class VectorRangeAttribute : PropertyAttribute
{
	public readonly (float minX, float maxX, float minY, float maxY, float minZ, float maxZ, float minW, float maxW) clamped;


	public VectorRangeAttribute
	(
		float minX = float.MinValue, float maxX = float.MaxValue,
		float minY = float.MinValue, float maxY = float.MaxValue,
		float minZ = float.MinValue, float maxZ = float.MaxValue,
		float minW = float.MinValue, float maxW = float.MaxValue
	)
	{
		clamped.minX = minX;
		clamped.maxX = maxX;

		clamped.minY = minY;
		clamped.maxY = maxY;

		clamped.minZ = minZ;
		clamped.maxZ = maxZ;

		clamped.minW = minW;
		clamped.maxW = maxW;
	}
}


#if UNITY_EDITOR

public sealed partial class VectorRangeAttribute
{ }

#endif
