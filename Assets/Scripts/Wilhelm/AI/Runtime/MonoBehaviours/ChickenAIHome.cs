using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public sealed partial class ChickenAIHome : HomeBase, ITarget, IFrameDependentPhysicsInteractor<ChickenAIHomePhysicsInteractionType>
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

	#region BabyChickenAI Other

	[NonSerialized]
	private readonly Queue<(ChickenAIHomePhysicsInteractionType triggerType, Collider2D collider2D, Collision2D collision2D)> physicsInteractionQueue = new();


	#endregion


	// Update
	private void Update()
	{
		TrySpawn(out _);
		TryRestoreHealth();
		DoFrameDependentPhysics();
	}

	public void RegisterFrameDependentPhysicsInteraction((ChickenAIHomePhysicsInteractionType triggerType, Collider2D collider2D, Collision2D collision2D) interaction)
	{
		if (!physicsInteractionQueue.Contains(interaction))
			physicsInteractionQueue.Enqueue(interaction);
	}

	public void DoFrameDependentPhysics()
	{
		for (int i = physicsInteractionQueue.Count - 1; i >= 0; i--)
		{
			var iteratedPhysicsInteraction = physicsInteractionQueue.Dequeue();

			switch (iteratedPhysicsInteraction.triggerType)
			{
				case ChickenAIHomePhysicsInteractionType.GateTriggerStay2D:
				DoGateTriggerStay2D(iteratedPhysicsInteraction);
				break;
			}
		}
	}

	private void DoGateTriggerStay2D((ChickenAIHomePhysicsInteractionType triggerType, Collider2D collider2D, Collision2D collision2D) interaction)
	{
		if (EventReflectorUtils.TryGetComponentByEventReflector<IHomeAccesser>(interaction.collider2D.gameObject, out IHomeAccesser foundAccesser))
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

	private void RestoreHealth()
	{
		Health = MaxHealth;
		IsDead = false;
	}

	private void TryRestoreHealth()
	{
		if (IsDead && restoreHealthTimer.Tick())
		{
			restoreHealthTimer.ResetAndRandomize();
			RestoreHealth();
		}
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
		=> RegisterFrameDependentPhysicsInteraction((ChickenAIHomePhysicsInteractionType.GateTriggerStay2D, collider, null));

	private void OnDead()
	{
		GameControllerSingleton.Instance.onTargetDeathDict[TargetType.ChickenHome]?.Invoke();

		// TODO: Decrease the extinction rate of Chickens
		IsDead = true;
	}


	// Dispose
	private void OnDisable()
	{
		DoFrameDependentPhysics();
	}
}


#if UNITY_EDITOR

public sealed partial class ChickenAIHome
{ }

#endif