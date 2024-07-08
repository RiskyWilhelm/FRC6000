using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public sealed partial class FoxAIHome : HomeBase, IFrameDependentPhysicsInteractor<FoxAIHomePhysicsInteractionType>
{
	[Header("FoxAIHome Spawn")]
	#region FoxAIHome Spawn

	public List<LuckTypeValue<AIPool>> nightDaySpawnPoolList = new();

	[SerializeField]
	private TimerRandomized nightDaySpawnTimer = new(10f, 10f, 20f);


	#endregion

	[Header("FoxAIHome Target Verify")]
	#region FoxAIHome Target Verify

	public List<TargetType> acceptedTargetTypeList = new();


	#endregion

	#region BabyChickenAI Other

	[NonSerialized]
	private readonly Queue<(FoxAIHomePhysicsInteractionType triggerType, Collider2D collider2D, Collision2D collision2D)> physicsInteractionQueue = new();


	#endregion


	// Update
	private void Update()
	{
		TrySpawn(out _);
		DoFrameDependentPhysics();
	}

	public void RegisterFrameDependentPhysicsInteraction((FoxAIHomePhysicsInteractionType triggerType, Collider2D collider2D, Collision2D collision2D) interaction)
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
				case FoxAIHomePhysicsInteractionType.GateTriggerStay2D:
				DoGateTriggerStay2D(iteratedPhysicsInteraction);
				break;

				case FoxAIHomePhysicsInteractionType.StealBabyChickenTriggerEnter2D:
				DoStealBabyChickenTriggerEnter2D(iteratedPhysicsInteraction);
				break;
			}
		}
	}

	private void DoGateTriggerStay2D((FoxAIHomePhysicsInteractionType triggerType, Collider2D collider2D, Collision2D collision2D) interaction)
	{
		if (!interaction.collider2D)
			return;

		if (EventReflectorUtils.TryGetComponentByEventReflector<IHomeAccesser>(interaction.collider2D.gameObject, out IHomeAccesser foundAccesser))
		{
			// Check if accesser wants to go in
			if (!foundAccesser.OpenAIHomeGate)
				return;

			// Accept only specific to enter
			if ((foundAccesser.ParentHome == this) && acceptedTargetTypeList.Contains(foundAccesser.TargetTag))
				foundAccesser.OnEnteredAIHome(this);
		}
	}

	private void DoStealBabyChickenTriggerEnter2D((FoxAIHomePhysicsInteractionType triggerType, Collider2D collider2D, Collision2D collision2D) interaction)
	{
		if (!interaction.collider2D)
			return;

		if (EventReflectorUtils.TryGetComponentByEventReflector<BabyChickenAI>(interaction.collider2D.gameObject, out BabyChickenAI foundTarget))
			foundTarget.OnStolenByFoxHome(this);
	}

	public override bool TrySpawn(out AIBase spawnedAI)
	{
		spawnedAI = null;

		if ((DayCycleControllerSingleton.Instance.GameTimeDaylightType is DaylightType.Night) && nightDaySpawnTimer.Tick())
		{
			nightDaySpawnTimer.ResetAndRandomize();
			return TrySpawnFromLuckList(nightDaySpawnPoolList, out spawnedAI);
		}

		return false;
	}

	public bool TrySpawnFromLuckList(in List<LuckTypeValue<AIPool>> luckPoolList, out AIBase spawnedAI)
	{
		spawnedAI = null;
		bool isSpawned = false;

		// Get auto-generated luck and all of the LuckAIPool elements where the LuckAIPool has the value of auto-generated luck
		var generatedLuckType = LuckUtils.Generate();
		var cachedLuckList = ListPool<LuckTypeValue<AIPool>>.Get();

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

		ListPool<LuckTypeValue<AIPool>>.Release(cachedLuckList);
		return isSpawned;
	}

	public void OnGateTriggerStay2D(Collider2D collider)
		=> RegisterFrameDependentPhysicsInteraction((FoxAIHomePhysicsInteractionType.GateTriggerStay2D, collider, null));

	public void OnStealBabyChickenTriggerEnter2D(Collider2D collider)
		=> RegisterFrameDependentPhysicsInteraction((FoxAIHomePhysicsInteractionType.StealBabyChickenTriggerEnter2D, collider, null));


	// Dispose
	private void OnDisable()
	{
		DoFrameDependentPhysics();
	}
}


#if UNITY_EDITOR

public sealed partial class FoxAIHome
{ }

#endif