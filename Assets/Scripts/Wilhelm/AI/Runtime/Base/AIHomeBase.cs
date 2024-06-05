using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public abstract partial class AIHomeBase : MonoBehaviour
{
	[Header("Spawn")]
	#region
	[SerializeField]
	private Timer nightSpawnTimer = new (20f);

	[SerializeField]
	private Timer lightSpawnTimer = new(10f);

	public List<Luck<AIPool>> luckAIPoolList = new ();
	#endregion


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
			TrySpawn(out _);
			break;
		}
	}

	protected virtual bool TrySpawn(out AIBase spawnedAI)
	{
		spawnedAI = null;
		bool isSpawned = false;

		// Get auto-generated luck and all of the LuckAIPool elements where the LuckAIPool has the value of auto-generated luck
		var generatedLuckType = LuckUtil.Generate();
		var cachedLuckList = ListPool<Luck<AIPool>>.Get();

        foreach (var iteratedLuckPool in luckAIPoolList)
        {
			if (iteratedLuckPool.luckType.HasFlag(generatedLuckType))
				cachedLuckList.Add(iteratedLuckPool);
        }

		// Try Spawn and release the cached list to the pool
		if (cachedLuckList.Count > 0)
		{
			var randomSelect = UnityEngine.Random.Range(0, cachedLuckList.Count);
			
			spawnedAI = cachedLuckList[randomSelect].value.Get(this.transform.position);

			if (spawnedAI is IAIHomeAccesser homeAccesser)
				homeAccesser.OnLeftFromAIHome(this);

			isSpawned = true;
		}

		ListPool<Luck<AIPool>>.Release(cachedLuckList);
		return isSpawned;
    }
}


#if UNITY_EDITOR

public abstract partial class AIHomeBase
{ }

#endif