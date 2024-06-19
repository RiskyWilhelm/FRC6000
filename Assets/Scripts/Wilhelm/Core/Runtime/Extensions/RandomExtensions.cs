using System;

public static class RandomExtensions
{
	public static double NextDouble(this Random random, double minInclusiveValue, double maxExclusiveValue)
	{
		return random.NextDouble() * (maxExclusiveValue - minInclusiveValue) + minInclusiveValue;
	}
}