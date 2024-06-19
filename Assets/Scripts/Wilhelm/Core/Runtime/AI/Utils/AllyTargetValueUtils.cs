using System.Collections.Generic;

public static class AllyTargetValueUtils
{
	public static bool TryGetByTargetTypeIn(IEnumerable<AlliedTargetTypeValue> allyTargetEnumerable, TargetType checkTargetType, out AlliedTargetTypeValue foundValue)
	{
        foundValue = default;

        foreach (var iteratedAllyTarget in allyTargetEnumerable)
        {
            if (iteratedAllyTarget.targetTag == checkTargetType)
            {
                foundValue = iteratedAllyTarget;
                return true;
            }
        }

        return false;
    }
}