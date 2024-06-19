using System;
using UnityEngine;

public static class LuckUtils
{
	public static LuckType Generate()
	{
		var generatedLuck = LuckType.VeryCommon;
		var generatedValue = UnityEngine.Random.value;

		// Check the possibilites.
		// For example:
		// (generatedValue = 0,7) and (posibility = 0.9)
		// In that case, (inner posibility = 0,9 - (0,9 * 0,25)) which equals (inner posibility = 0,675)
		for (int i = (int)LuckType.Impossible; i >= (int)LuckType.VeryCommon; i >>= 1)
        {
			// Get rid of division by zero error
			if (i == 0)
				continue;

			if (Enum.IsDefined(typeof(LuckType), i))
			{
				var possibility = (1f / Math.Abs(i));
				var selectionSize = 0.25f;
				var innerPosibility = Mathf.Clamp01(possibility - (possibility * selectionSize));
				var isValid = (generatedValue <= possibility) && (generatedValue >= innerPosibility);

				if (isValid)
				{
					generatedLuck = (LuckType)i;
					break;
				}
			}
		}

		return generatedLuck;
	}
}