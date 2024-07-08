using System;

public static class RandomExtensions
{
	public static float NextFloat(this Random random, float minInclusiveValue, float maxExclusiveValue)
		=> (float)random.NextDouble(minInclusiveValue, maxExclusiveValue);

	public static double NextDouble(this Random random, double minInclusiveValue, double maxExclusiveValue)
	{
		return random.NextDouble() * (maxExclusiveValue - minInclusiveValue) + minInclusiveValue;
	}
}