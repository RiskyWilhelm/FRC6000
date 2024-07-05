using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public sealed partial class WarrirorFoxAI : GroundedAIBase, IHomeAccesser, IFrameDependentPhysicsInteractor<WarrirorFoxAIPhysicsInteractionType>
{
	[Header("WarrirorFoxAI Movement")]
	#region BabyFoxAI Movement

	[SerializeField]
	private TimerRandomized goHomeBackTimer = new(10f, 10f, 20f);


	#endregion

	[Header("WarrirorFoxAI Single Normal Attack")]
	#region WarrirorFoxAI Single Normal Attack

	[SerializeField]
	private uint singleNormalAttackDamage = 1;

	[SerializeField]
	private TimerRandomized singleNormalAttackTimer = new(0.5f, 0.75f);

	[NonSerialized]
	private (ITarget target, Coroutine coroutine) currentSingleNormalAttack;


	#endregion

	#region WarrirorFoxAI Target

	[NonSerialized]
	private readonly HashSet<ITargetValue<Transform>> targetInRangeSet = new();


	#endregion

	#region WarrirorFoxAI Other

	[field: NonSerialized]
	public bool IsCaughtMeal {  get; private set; }

	[field: NonSerialized]
	public bool OpenAIHomeGate { get; private set; }

	[field: NonSerialized]
	public HomeBase ParentHome { get; set; }

	[NonSerialized]
	private readonly Queue<(WarrirorFoxAIPhysicsInteractionType triggerType, Collider2D collider2D, Collision2D collision2D)> physicsInteractionQueue = new();


	#endregion


	// Initialize
	protected override void OnEnable()
	{
		goHomeBackTimer.ResetAndRandomize();
		UpdateByDaylightType(DayCycleControllerSingleton.Instance.Time.daylightType);
		RefreshAttackState();

		base.OnEnable();
	}

	private void Start()
	{
		DayCycleControllerSingleton.Instance.onDaylightTypeChanged.AddListener(OnDaylightTypeChanged);
	}


	// Update
	protected override void Update()
	{
		goHomeBackTimer.Tick();

		if (State is PlayerStateType.Attacking)
			goHomeBackTimer.Reset();

		DoFrameDependentPhysics();
		base.Update();
	}

	private void UpdateByDaylightType(DaylightType newDaylightType)
	{
		switch (newDaylightType)
		{
			case DaylightType.Light:
			{
				acceptedTargetTypeList.Remove(TargetType.ChickenHome);
			}
			break;

			case DaylightType.Night:
			{
				if (!acceptedTargetTypeList.Contains(TargetType.ChickenHome))
					acceptedTargetTypeList.Add(TargetType.ChickenHome);
			}
			break;
		}
	}

	public void RegisterFrameDependentPhysicsInteraction((WarrirorFoxAIPhysicsInteractionType triggerType, Collider2D collider2D, Collision2D collision2D) interaction)
	{
		if (!physicsInteractionQueue.Contains(interaction))
			physicsInteractionQueue.Enqueue(interaction);
	}

	public bool TrySetDestinationToHome()
	{
		if (ParentHome != null)
			return OpenAIHomeGate = this.TrySetDestinationToTransform(ParentHome.transform, 0.1f);

		return false;
	}

	public bool TrySetDestinationAwayFromOrToNearestTarget()
	{
		// Let the blocked states take control
		if (State is PlayerStateType.Jumping or PlayerStateType.Attacking or PlayerStateType.Defending or PlayerStateType.Dead)
			return false;

		var isSetDestination = false;
		var cachedRunawayTransformList = ListPool<Transform>.Get();
		var cachedAcceptedTransformList = ListPool<Transform>.Get();

		// Select valid targets in range
		foreach (var iteratedTargetValue in targetInRangeSet)
		{
			if (IsAbleToGoToVector(iteratedTargetValue.value.position))
			{
				if (runawayTargetTypeList.Contains(iteratedTargetValue.target.TargetTag))
					cachedRunawayTransformList.Add(iteratedTargetValue.value);

				if (acceptedTargetTypeList.Contains(iteratedTargetValue.target.TargetTag))
					cachedAcceptedTransformList.Add(iteratedTargetValue.value);
			}
		}

		// If there is any runaway target in range, runaway from it and discard other targets
		if (cachedRunawayTransformList.Count > 0)
		{
			this.transform.TryGetNearestTransform(cachedRunawayTransformList.GetEnumerator(), out Transform nearestTransform);
			isSetDestination = TrySetDestinationAwayFromVector(nearestTransform.position);
		}

		// If there is any accepted target and cant runaway, catch the nearest target
		if (!IsCaughtMeal && !isSetDestination && (cachedAcceptedTransformList.Count > 0))
		{
			this.transform.TryGetNearestTransform(cachedAcceptedTransformList.GetEnumerator(), out Transform nearestTransform);
			isSetDestination = TrySetDestinationToVector(nearestTransform.position);
		}

		if (isSetDestination)
			State = PlayerStateType.Running;

		ListPool<Transform>.Release(cachedRunawayTransformList);
		ListPool<Transform>.Release(cachedAcceptedTransformList);
		return isSetDestination;
	}

	private bool TryDoSingleNormalAttackTo(ITarget target)
	{
		// Prevents StartCoroutine to get called when self is disabled
		if (!this.gameObject.activeSelf)
			return false;

		// If attacking, let it finish first
		if (State == PlayerStateType.Attacking)
			return false;

		// If target is not accepted, return
		if (!acceptedTargetTypeList.Contains(target.TargetTag))
			return false;

		// Do the attack
		currentSingleNormalAttack.target = target;
		currentSingleNormalAttack.coroutine = StartCoroutine(DoSingleNormalAttack(target));
		return true;
	}

	private bool TryCancelSingleNormalAttackFrom(ITarget target)
	{
		if (target == currentSingleNormalAttack.target)
		{
			RefreshSingleNormalAttack();
			return true;
		}

		return false;
	}

	private void RefreshSingleNormalAttack()
	{
		if (currentSingleNormalAttack.coroutine != null)
			StopCoroutine(currentSingleNormalAttack.coroutine);

		if (State == PlayerStateType.Attacking)
			State = PlayerStateType.Idle;

		currentSingleNormalAttack = default;
		singleNormalAttackTimer.ResetAndRandomize();
	}

	private void RefreshAttackState()
	{
		RefreshSingleNormalAttack();
	}

	private IEnumerator DoSingleNormalAttack(ITarget target)
	{
		// Cant attack the dead
		if (target.IsDead)
			yield break;

		// Take full control over the body by setting state to attacking, ready timer
		State = PlayerStateType.Attacking;

		// Do the attack
		target.TakeDamage(singleNormalAttackDamage, selfRigidbody.position);

		if (target.IsDead)
		{
			OnKilledTarget(target);
			RefreshSingleNormalAttack();
			yield break;
		}

		// Lock the state to Attacking until timer ends
		while (!singleNormalAttackTimer.Tick())
			yield return null;

		RefreshSingleNormalAttack();
	}

	public void DoFrameDependentPhysics()
	{
		for (int i = physicsInteractionQueue.Count - 1; i >= 0; i--)
		{
			var iteratedPhysicsInteraction = physicsInteractionQueue.Dequeue();

			switch (iteratedPhysicsInteraction.triggerType)
			{
				case WarrirorFoxAIPhysicsInteractionType.EnemyTriggerEnter2D:
				DoEnemyTriggerEnter2D(iteratedPhysicsInteraction);
				break;

				case WarrirorFoxAIPhysicsInteractionType.EnemyTriggerStay2D:
				DoEnemyTriggerStay2D(iteratedPhysicsInteraction);
				break;

				case WarrirorFoxAIPhysicsInteractionType.EnemyTriggerExit2D:
				DoEnemyTriggerExit2D(iteratedPhysicsInteraction);
				break;

				case WarrirorFoxAIPhysicsInteractionType.SingleNormalAttackTriggerStay2D:
				DoSingleNormalAttackTriggerStay2D(iteratedPhysicsInteraction);
				break;

				case WarrirorFoxAIPhysicsInteractionType.SingleNormalAttackTriggerExit2D:
				DoSingleNormalAttackTriggerExit2D(iteratedPhysicsInteraction);
				break;
			}
		}
	}

	private void DoEnemyTriggerEnter2D((WarrirorFoxAIPhysicsInteractionType triggerType, Collider2D collider2D, Collision2D collision2D) interaction)
	{
		if (!interaction.collider2D)
			return;

		if (EventReflectorUtils.TryGetComponentByEventReflector<ITarget>(interaction.collider2D.gameObject, out ITarget foundTarget))
			targetInRangeSet.Add(new ITargetValue<Transform>(foundTarget, (foundTarget as Component).transform));
	}

	private void DoEnemyTriggerStay2D((WarrirorFoxAIPhysicsInteractionType triggerType, Collider2D collider2D, Collision2D collision2D) interaction)
	{
		if (!interaction.collider2D)
			return;

		TrySetDestinationAwayFromOrToNearestTarget();
	}

	private void DoEnemyTriggerExit2D((WarrirorFoxAIPhysicsInteractionType triggerType, Collider2D collider2D, Collision2D collision2D) interaction)
	{
		if (!interaction.collider2D)
		{
			targetInRangeSet.RemoveWhere((iteratedTargetTuple) => !iteratedTargetTuple.value);
			return;
		}

		if (EventReflectorUtils.TryGetComponentByEventReflector<ITarget>(interaction.collider2D.gameObject, out ITarget foundTarget))
			targetInRangeSet.Remove(new ITargetValue<Transform>(foundTarget, (foundTarget as Component).transform));
	}

	private void DoSingleNormalAttackTriggerStay2D((WarrirorFoxAIPhysicsInteractionType triggerType, Collider2D collider2D, Collision2D collision2D) interaction)
	{
		if (!interaction.collider2D)
			return;

		if ((State != PlayerStateType.Attacking) && EventReflectorUtils.TryGetComponentByEventReflector<ITarget>(interaction.collider2D.gameObject, out ITarget foundTarget))
			TryDoSingleNormalAttackTo(foundTarget);
	}

	private void DoSingleNormalAttackTriggerExit2D((WarrirorFoxAIPhysicsInteractionType triggerType, Collider2D collider2D, Collision2D collision2D) interaction)
	{
		if (!interaction.collider2D)
			return;

		if (EventReflectorUtils.TryGetComponentByEventReflector<ITarget>(interaction.collider2D.gameObject, out ITarget escapedTarget))
			TryCancelSingleNormalAttackFrom(escapedTarget);
	}

	protected override void DoIdle()
	{
		// If not grounded, set state to Flying
		if (!IsGrounded())
		{
			goHomeBackTimer.ResetAndRandomize();
			State = PlayerStateType.Flying;
			return;
		}

		// If wants to go home, set state to walking or running depending on the meal state
		if (goHomeBackTimer.HasEnded || IsCaughtMeal)
		{
			goHomeBackTimer.ResetAndRandomize();

			if (TrySetDestinationToHome())
			{
				if (IsCaughtMeal)
					State = PlayerStateType.Running;
				else
					State = PlayerStateType.Walking;

				return;
			}
		}

		// Otherwise, continue old idle
		base.DoIdle();
	}

	protected override void OnStateChangedToDead()
	{
		PlayerControllerSingleton.Instance.onTargetDeathEventDict[TargetType.WarrirorFox]?.Invoke();
		ReleaseOrDestroySelf();
		base.OnStateChangedToDead();
	}

	private void OnDaylightTypeChanged(DaylightType newDaylightType)
	{
		UpdateByDaylightType(newDaylightType);
	}

	protected override void OnChangedDestination(Vector2? newDestination)
	{
		OpenAIHomeGate = false;
		base.OnChangedDestination(newDestination);
	}

	private void OnKilledTarget(ITarget target)
	{
		// If target is a chicken or chicken home, it means it caught a chicken
		if (target.TargetTag is TargetType.BabyChicken or TargetType.ChickenHome)
		{
			IsCaughtMeal = true;
			TrySetDestinationToHome();
		}
	}

	public void OnEnteredAIHome(HomeBase home)
	{
		IsCaughtMeal = false;
		ReleaseOrDestroySelf();
	}

	public void OnLeftFromAIHome(HomeBase home)
	{
		OpenAIHomeGate = false;
	}

	public void OnSingleNormalAttackTriggerStay2D(Collider2D collider)
		=> RegisterFrameDependentPhysicsInteraction((WarrirorFoxAIPhysicsInteractionType.SingleNormalAttackTriggerStay2D, collider, null));

	public void OnSingleNormalAttackTriggerExit2D(Collider2D collider)
		=> RegisterFrameDependentPhysicsInteraction((WarrirorFoxAIPhysicsInteractionType.SingleNormalAttackTriggerExit2D, collider, null));

	public void OnEnemyTriggerEnter2D(Collider2D collider)
		=> RegisterFrameDependentPhysicsInteraction((WarrirorFoxAIPhysicsInteractionType.EnemyTriggerEnter2D, collider, null));

	public void OnEnemyTriggerStay2D(Collider2D collider)
		=> RegisterFrameDependentPhysicsInteraction((WarrirorFoxAIPhysicsInteractionType.EnemyTriggerStay2D, collider, null));

	public void OnEnemyTriggerExit2D(Collider2D collider)
		=> RegisterFrameDependentPhysicsInteraction((WarrirorFoxAIPhysicsInteractionType.EnemyTriggerExit2D, collider, null));

	public override void CopyTo(in AIBase main)
	{
		if (main is WarrirorFoxAI foundSelf)
		{
			// Movement
			foundSelf.goHomeBackTimer = this.goHomeBackTimer;

			// Normal Attack
			foundSelf.singleNormalAttackTimer = this.singleNormalAttackTimer;
			foundSelf.singleNormalAttackDamage = this.singleNormalAttackDamage;
		}

		base.CopyTo(main);
	}

	[ContextMenu(nameof(LogTargetCount))]
	public void LogTargetCount()
	{
		Debug.LogFormat("Target In Range Count: {0}", targetInRangeSet.Count);
    }

	[ContextMenu(nameof(LogPhysicsInteractionCount))]
	public void LogPhysicsInteractionCount()
	{
		Debug.LogFormat("Target In Range Count: {0}", physicsInteractionQueue.Count);
	}


	// Dispose
	protected override void OnDisable()
	{
		if (GameControllerSingleton.IsQuitting)
			return;

		DoFrameDependentPhysics();
		base.OnDisable();
	}

	private void OnDestroy()
	{
		if (GameControllerSingleton.IsQuitting)
			return;

		DayCycleControllerSingleton.Instance.onDaylightTypeChanged.RemoveListener(OnDaylightTypeChanged);
	}
}


#if UNITY_EDITOR

public sealed partial class WarrirorFoxAI
{ }

#endif