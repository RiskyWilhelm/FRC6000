using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public sealed partial class ChickenAIHome : HomeBase, ITarget
{
	[Header("ChickenAIHome Spawn")]
	#region ChickenAIHome Spawn

	public List<LuckValue<AIPool>> lightDaySpawnPoolList = new();

	public List<LuckValue<AIPool>> nightDaySpawnPoolList = new();

	[SerializeField]
	private TimerRandomized lightDaySpawnTimer = new(10f, 10f, 20f);

	[SerializeField]
	private TimerRandomized nightDaySpawnTimer = new(10f, 10f, 20f);


	#endregion

	#region ChickenAIHome Target Verify

	public List<TargetType> acceptedTargetTypeList = new ();


	#endregion

	#region ChickenAIHome Stats

	[SerializeField]
	private TimerRandomized restoreHealthTimer = new(5f, 0f, 5f);

	[field: SerializeField]
	public uint Health { get; private set; }

	[field: SerializeField]
	public uint MaxHealth { get; private set; }

	[field: NonSerialized]
	public bool IsDead { get; private set; }

	public TargetType TargetTag => TargetType.ChickenHome;


	#endregion


	// Update
	private void Update()
	{
		TrySpawn(out _);
		TryRestoreHealth();
	}

	public override bool TrySpawn(out AIBase spawnedAI)
	{
		spawnedAI = null;

		switch (DayCycleControllerSingleton.Instance.Time.daylightType)
		{
			case DaylightType.Light:
			{
				if (lightDaySpawnTimer.Tick())
				{
					lightDaySpawnTimer.ResetAndRandomize();
					return TrySpawnFromLuckList(in lightDaySpawnPoolList, out spawnedAI);
				}
			}
			break;

			case DaylightType.Night:
			{
				if (nightDaySpawnTimer.Tick())
				{
					nightDaySpawnTimer.ResetAndRandomize();
					return TrySpawnFromLuckList(in nightDaySpawnPoolList, out spawnedAI);
				}
			}
			break;
		}

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

			// Accept only specific types
			if ((foundAccesser.ParentHome == this) && acceptedTargetTypeList.Contains(foundAccesser.TargetTag))
				foundAccesser.OnEnteredAIHome(this);
		}
	}

	public void TakeDamage(uint damage, Vector2 occurWorldPosition)
	{
		Health = (ushort)Math.Clamp(Health - (int)damage, ushort.MinValue, ushort.MaxValue);

		if ((Health == ushort.MinValue) && !IsDead)
			OnDead();
	}

	private void TryRestoreHealth()
	{
		if (IsDead && restoreHealthTimer.Tick())
			RestoreHealth();
	}

	private void RestoreHealth()
	{
		Health = MaxHealth;
		IsDead = false;
	}

	private void OnDead()
	{
		GameControllerSingleton.Instance.onChickenDeath?.Invoke();

		// TODO: Decrease the extinction rate of Chickens
		IsDead = true;
	}
}


#if UNITY_EDITOR

public sealed partial class ChickenAIHome
{ }

#endif