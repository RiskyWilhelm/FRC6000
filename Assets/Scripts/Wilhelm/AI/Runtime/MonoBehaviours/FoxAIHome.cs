using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public sealed partial class FoxAIHome : HomeBase
{
	[Header("FoxAIHome Spawn")]
	#region FoxAIHome Spawn

	public List<LuckValue<AIPool>> nightDaySpawnPoolList = new();

	[SerializeField]
	private TimerRandomized nightDaySpawnTimer = new(10f, 10f, 20f);


	#endregion

	[Header("FoxAIHome Target Verify")]
	#region FoxAIHome Target Verify

	public List<TargetType> acceptedTargetTypeList = new();


	#endregion


	// Update
	private void Update()
	{
		TrySpawn(out _);
	}

	public override bool TrySpawn(out AIBase spawnedAI)
	{
		spawnedAI = null;

		if ((DayCycleControllerSingleton.Instance.Time.daylightType is DaylightType.Night) && nightDaySpawnTimer.Tick())
			return TrySpawnFromLuckList(nightDaySpawnPoolList, out spawnedAI);

		return false;
	}

	public bool TrySpawnFromLuckList(in List<LuckValue<AIPool>> luckPoolList, out AIBase spawnedAI)
	{
		spawnedAI = null;
		bool isSpawned = false;

		// Get auto-generated luck and all of the LuckAIPool elements where the LuckAIPool has the value of auto-generated luck
		var generatedLuckType = LuckUtils.Generate();
		var cachedLuckList = ListPool<LuckValue<AIPool>>.Get();

		foreach (var iteratedLuckPool in luckPoolList)
		{
			if (iteratedLuckPool.luckType.HasFlag(generatedLuckType))
				cachedLuckList.Add(iteratedLuckPool);
		}

		// Try Spawn and release the cached list to the pool
		if (cachedLuckList.Count > 0)
		{
			var randomSelect = UnityEngine.Random.Range(0, cachedLuckList.Count);
			spawnedAI = cachedLuckList[randomSelect].value.Get(this.transform.position);

			// TODO: This is ridicilous
			if (spawnedAI is IHomeAccesser homeAccesser)
			{
				homeAccesser.OnLeftFromAIHome(this);
				homeAccesser.ParentHome = this;
			}

			isSpawned = true;
		}

		ListPool<LuckValue<AIPool>>.Release(cachedLuckList);
		return isSpawned;
	}

	public void OnGateTriggerStay2D(Collider2D collider)
	{
		if (EventReflectorUtils.TryGetComponentByEventReflector<IHomeAccesser>(collider.gameObject, out IHomeAccesser foundAccesser))
		{
			// Check if accesser wants to go in
			if (!foundAccesser.OpenAIHomeGate)
				return;

			// Accept only FoxAI to enter
			if ((foundAccesser.ParentHome == this) && acceptedTargetTypeList.Contains(foundAccesser.TargetTag))
				foundAccesser.OnEnteredAIHome(this);
		}
	}
}


#if UNITY_EDITOR

public sealed partial class FoxAIHome
{ }

#endif