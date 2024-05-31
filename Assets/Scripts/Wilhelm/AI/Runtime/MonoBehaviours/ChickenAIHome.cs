using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public sealed partial class ChickenAIHome : MonoBehaviour
{
	[Header("Spawn")]
	[SerializeField]
	private Timer nightSpawnTimer = new (30f);

	[SerializeField]
	private Timer lightSpawnTimer = new(10f);

	public List<Luck<AIPool>> chickenLuckPoolList = new ();


	// Update
	private void Update()
	{
		switch (DayCycleControllerSingleton.Instance.Time.daylightType)
		{
			case DayLightType.Night:
			{
				if (nightSpawnTimer.Tick())
					goto default;
			}
			break;

			case DayLightType.Light:
			{
				if (lightSpawnTimer.Tick())
					goto default;
			}
			break;

			default:
			SpawnChicken();
			break;
		}
	}

	private void SpawnChicken()
	{
		// Get auto-generated luck and get chicken pool list by that
		var generatedLuckType = LuckUtil.Generate();
		var cachedLuckList = ListPool<Luck<AIPool>>.Get();

        foreach (var iteratedLuckPool in chickenLuckPoolList)
        {
			if (iteratedLuckPool.luckType.HasFlag(generatedLuckType))
				cachedLuckList.Add(iteratedLuckPool);
        }

		// Spawn and release the cached list to the pool
		if (cachedLuckList.Count > 0)
		{
			var randomSelect = UnityEngine.Random.Range(0, cachedLuckList.Count);
			cachedLuckList[randomSelect].value.Get(this.transform.position);
		}

		ListPool<Luck<AIPool>>.Release(cachedLuckList);
    }
}


#if UNITY_EDITOR

public sealed partial class ChickenAIHome
{ }

#endif