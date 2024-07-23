using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public abstract partial class GroundedWarrirorAIBase : GroundedAIBase, IHomeAccesser, IFrameDependentPhysicsInteractor<GroundedWarrirorAIPhysicsInteractionType>
{
	[Header("GroundedWarrirorAIBase Movement")]
	#region GroundedWarrirorAIBase Movement

	[SerializeField]
	protected TimerRandomized goHomeBackTimer;


	#endregion

	[Header("GroundedWarrirorAIBase Single Normal Attack")]
	#region GroundedWarrirorAIBase Single Normal Attack

	[SerializeField]
	private uint singleNormalAttackDamage;

	[SerializeField]
	[Tooltip("Sticks the state")]
	protected Timer singleNormalAttackBlockTimer;

	[NonSerialized]
	private ITargetValue<Coroutine> currentSingleNormalAttack;

	public bool IsSingleNormalAttacking => (currentSingleNormalAttack != default);


	#endregion

	#region GroundedWarrirorAIBase Target

	[NonSerialized]
	protected readonly HashSet<ITargetValue<Transform>> targetInRangeSet = new();


	#endregion

	#region GroundedWarrirorAIBase Other

	[field: NonSerialized]
	public bool OpenAIHomeGate { get; protected set; }

	[field: NonSerialized]
	public HomeBase ParentHome { get; set; }

	[NonSerialized]
	private readonly Queue<(GroundedWarrirorAIPhysicsInteractionType triggerType, Collider2D collider2D, Collision2D collision2D)> groundedWarrirorAIPhysicsInteractionQueue = new();


	#endregion


	// Initialize
	protected override void OnEnable()
	{
		goHomeBackTimer.ResetAndRandomize();
		CancelAllAttacks();

		base.OnEnable();
	}


	// Update
	protected override void Update()
	{
		if (goHomeBackTimer.Tick())
			OpenAIHomeGate = true;

		DoFrameDependentPhysics();
		base.Update();
	}

	public void RegisterFrameDependentPhysicsInteraction((GroundedWarrirorAIPhysicsInteractionType triggerType, Collider2D collider2D, Collision2D collision2D) interaction)
	{
		if (!groundedWarrirorAIPhysicsInteractionQueue.Contains(interaction))
			groundedWarrirorAIPhysicsInteractionQueue.Enqueue(interaction);
	}

	public virtual void ForceToGoHome()
	{
		goHomeBackTimer.Finish();
		TrySetDestinationToHome();
	}


	public bool TrySetDestinationToHome()
	{
		if (ParentHome != null)
			return OpenAIHomeGate = this.TrySetDestinationToTransform(ParentHome.transform, 0.1f);

		return false;
	}

	public bool TrySetDestinationToNearestTarget()
	{
		var isSetDestination = false;
		var cachedAcceptedTransformList = ListPool<Transform>.Get();

		// Select valid targets in range
		foreach (var iteratedTargetValue in targetInRangeSet)
		{
			if (IsAbleToGoToVector(iteratedTargetValue.value.position) && acceptedTargetTypeList.Contains(iteratedTargetValue.target.TargetTag))
				cachedAcceptedTransformList.Add(iteratedTargetValue.value);
		}

		// If there is any accepted target and cant runaway, catch the nearest target
		if (cachedAcceptedTransformList.Count > 0)
		{
			this.transform.TryGetNearestTransform(cachedAcceptedTransformList.GetEnumerator(), out Transform nearestTransform);
			isSetDestination = TrySetDestinationToVector(nearestTransform.position, considerAsReachedDistance: 1.5f);
		}

		if (isSetDestination && !IsSingleNormalAttacking && (State is not PlayerStateType.Jumping or PlayerStateType.Flying or PlayerStateType.Attacking or PlayerStateType.Blocked or PlayerStateType.Dead))
			State = PlayerStateType.Running;

		ListPool<Transform>.Release(cachedAcceptedTransformList);
		return isSetDestination;
	}

	public bool TrySetDestinationAwayFromNearestTarget()
	{
		// Let the blocked states take control
		var isSetDestination = false;
		var cachedRunawayTransformList = ListPool<Transform>.Get();

		// Select valid targets in range
		foreach (var iteratedTargetValue in targetInRangeSet)
		{
			if (IsAbleToGoToVector(iteratedTargetValue.value.position) && runawayTargetTypeList.Contains(iteratedTargetValue.target.TargetTag))
				cachedRunawayTransformList.Add(iteratedTargetValue.value);
		}

		// If there is any runaway target in range, runaway from it and discard other targets
		if (cachedRunawayTransformList.Count > 0)
		{
			this.transform.TryGetNearestTransform(cachedRunawayTransformList.GetEnumerator(), out Transform nearestTransform);
			isSetDestination = TrySetDestinationAwayFromVector(nearestTransform.position, isGroundedOnly: true);
		}

		if (isSetDestination && !IsSingleNormalAttacking && (State is not PlayerStateType.Jumping or PlayerStateType.Flying or PlayerStateType.Attacking or PlayerStateType.Blocked or PlayerStateType.Dead))
			State = PlayerStateType.Running;

		ListPool<Transform>.Release(cachedRunawayTransformList);
		return isSetDestination;
	}

	public virtual bool TrySetDestinationAwayFromOrToNearestTarget()
	{
		return TrySetDestinationAwayFromNearestTarget() || TrySetDestinationToNearestTarget();
	}

	protected bool TryDoSingleNormalAttackTo(ITarget target)
	{
		// Prevents StartCoroutine to get called when self is disabled
		if (!this.gameObject.activeSelf)
			return false;

		// If attacking or target dead, let it finish first
		if (IsSingleNormalAttacking || target.IsDead)
			return false;

		if (State is PlayerStateType.Jumping or PlayerStateType.Flying or PlayerStateType.Attacking or PlayerStateType.Blocked or PlayerStateType.Dead)
			return false;

		// If target is not accepted, return
		if (!acceptedTargetTypeList.Contains(target.TargetTag))
			return false;

		// Do the attack
		currentSingleNormalAttack.target = target;
		currentSingleNormalAttack.value = StartCoroutine(DoSingleNormalAttack(target));
		return true;
	}

	protected bool TryCancelSingleNormalAttackFrom(ITarget target)
	{
		if (target == currentSingleNormalAttack.target)
		{
			CancelSingleNormalAttack();
			return true;
		}

		return false;
	}

	protected void CancelSingleNormalAttack()
	{
		if (currentSingleNormalAttack.value != null)
			StopCoroutine(currentSingleNormalAttack.value);

		if (State is PlayerStateType.Attacking)
			State = PlayerStateType.Idle;

		currentSingleNormalAttack = default;
		singleNormalAttackBlockTimer.Reset();
	}

	protected void CancelAllAttacks()
	{
		CancelSingleNormalAttack();
	}

	protected IEnumerator DoSingleNormalAttack(ITarget target)
	{
		// Take full control over the body by setting state to attacking, ready timer
		State = PlayerStateType.Attacking;

		// Lock the state to Attacking until timer ends
		while (!singleNormalAttackBlockTimer.Tick())
			yield return null;

		// Target is killed by unknown thing while attacking
		if (target.IsDead)
		{
			CancelSingleNormalAttack();
			yield break;
		}

		// Do the attack
		target.TakeDamage(singleNormalAttackDamage, SelfRigidbody.position);

		if (target.IsDead)
			OnKilledTarget(target);

		CancelSingleNormalAttack();
	}

	public void DoFrameDependentPhysics()
	{
		for (int i = groundedWarrirorAIPhysicsInteractionQueue.Count - 1; i >= 0; i--)
		{
			var iteratedPhysicsInteraction = groundedWarrirorAIPhysicsInteractionQueue.Dequeue();

			switch (iteratedPhysicsInteraction.triggerType)
			{
				case GroundedWarrirorAIPhysicsInteractionType.EnemyTriggerEnter2D:
				DoEnemyTriggerEnter2D(iteratedPhysicsInteraction);
				break;

				case GroundedWarrirorAIPhysicsInteractionType.EnemyTriggerStay2D:
				DoEnemyTriggerStay2D(iteratedPhysicsInteraction);
				break;

				case GroundedWarrirorAIPhysicsInteractionType.EnemyTriggerExit2D:
				DoEnemyTriggerExit2D(iteratedPhysicsInteraction);
				break;

				case GroundedWarrirorAIPhysicsInteractionType.SingleNormalAttackTriggerStay2D:
				DoSingleNormalAttackTriggerStay2D(iteratedPhysicsInteraction);
				break;

				case GroundedWarrirorAIPhysicsInteractionType.SingleNormalAttackTriggerExit2D:
				DoSingleNormalAttackTriggerExit2D(iteratedPhysicsInteraction);
				break;
			}
		}
	}

	private void DoEnemyTriggerEnter2D((GroundedWarrirorAIPhysicsInteractionType triggerType, Collider2D collider2D, Collision2D collision2D) interaction)
	{
		if (!interaction.collider2D)
			return;

		if (EventReflectorUtils.TryGetComponentByEventReflector<ITarget>(interaction.collider2D.gameObject, out ITarget foundTarget))
			targetInRangeSet.Add(new ITargetValue<Transform>(foundTarget, (foundTarget as Component).transform));
	}

	private void DoEnemyTriggerStay2D((GroundedWarrirorAIPhysicsInteractionType triggerType, Collider2D collider2D, Collision2D collision2D) interaction)
	{
		if (!interaction.collider2D)
			return;

		TrySetDestinationAwayFromOrToNearestTarget();
	}

	private void DoEnemyTriggerExit2D((GroundedWarrirorAIPhysicsInteractionType triggerType, Collider2D collider2D, Collision2D collision2D) interaction)
	{
		if (!interaction.collider2D)
		{
			targetInRangeSet.RemoveWhere((iteratedTargetTuple) => !iteratedTargetTuple.value);
			return;
		}

		if (EventReflectorUtils.TryGetComponentByEventReflector<ITarget>(interaction.collider2D.gameObject, out ITarget foundTarget))
			targetInRangeSet.Remove(new ITargetValue<Transform>(foundTarget, (foundTarget as Component).transform));
	}

	private void DoSingleNormalAttackTriggerStay2D((GroundedWarrirorAIPhysicsInteractionType triggerType, Collider2D collider2D, Collision2D collision2D) interaction)
	{
		if (!interaction.collider2D)
			return;

		if (!IsSingleNormalAttacking && EventReflectorUtils.TryGetComponentByEventReflector<ITarget>(interaction.collider2D.gameObject, out ITarget foundTarget))
			TryDoSingleNormalAttackTo(foundTarget);
	}

	private void DoSingleNormalAttackTriggerExit2D((GroundedWarrirorAIPhysicsInteractionType triggerType, Collider2D collider2D, Collision2D collision2D) interaction)
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
			State = PlayerStateType.Flying;
			return;
		}

		// If wants to go home, set state to walking
		if (goHomeBackTimer.HasEnded)
		{
			if (TrySetDestinationToHome())
			{
				State = PlayerStateType.Walking;
				return;
			}
		}

		// Otherwise, continue old idle
		base.DoIdle();
	}

	// TODO: Same implementation as DoRunning except there is no switching to Idle
	protected override void DoAttacking()
	{
		// If not grounded, set state to flying
		if (!IsGrounded())
		{
			State = PlayerStateType.Flying;
			return;
		}

		// If destination available, set state to running
		if (TryGetDestination(out Vector2 worldDestination))
		{
			// If wants to jump, jump
			if (IsAbleToJumpTowardsDestination())
			{
				Jump();
				return;
			}

			// Set the horizontal direction that mirrors to FixedUpdate
			norDirHorizontal = (sbyte)Mathf.Sign((worldDestination - SelfRigidbody.position).x);
			return;
		}
	}

	protected override void DoAttackingFixed()
		=> base.DoRunningFixed();

	protected override void OnStateChangedToAttacking()
	{
		goHomeBackTimer.CurrentSecond = Mathf.Clamp(goHomeBackTimer.CurrentSecond + 5f, 0f, goHomeBackTimer.TickSecond);
	}

	protected override void OnStateChangedToDead()
	{
		ReleaseOrDestroySelf();
	}

	protected override void OnStateChangedToAny(PlayerStateType newState)
	{
		if (newState is not PlayerStateType.Attacking)
			CancelAllAttacks();
	}

	protected override void OnChangedDestination(Vector2? newDestination)
	{
		if (!goHomeBackTimer.HasEnded)
			OpenAIHomeGate = false;

		base.OnChangedDestination(newDestination);
	}

	protected virtual void OnKilledTarget(ITarget target)
	{
		TrySetDestinationToHome();
	}

	public virtual void OnEnteredAIHome(HomeBase home)
	{
		ReleaseOrDestroySelf();
	}

	public virtual void OnLeftFromAIHome(HomeBase home)
	{
		OpenAIHomeGate = false;
	}

	public void OnSingleNormalAttackTriggerStay2D(Collider2D collider)
		=> RegisterFrameDependentPhysicsInteraction((GroundedWarrirorAIPhysicsInteractionType.SingleNormalAttackTriggerStay2D, collider, null));

	public void OnSingleNormalAttackTriggerExit2D(Collider2D collider)
		=> RegisterFrameDependentPhysicsInteraction((GroundedWarrirorAIPhysicsInteractionType.SingleNormalAttackTriggerExit2D, collider, null));

	public void OnEnemyTriggerEnter2D(Collider2D collider)
		=> RegisterFrameDependentPhysicsInteraction((GroundedWarrirorAIPhysicsInteractionType.EnemyTriggerEnter2D, collider, null));

	public void OnEnemyTriggerStay2D(Collider2D collider)
		=> RegisterFrameDependentPhysicsInteraction((GroundedWarrirorAIPhysicsInteractionType.EnemyTriggerStay2D, collider, null));

	public void OnEnemyTriggerExit2D(Collider2D collider)
		=> RegisterFrameDependentPhysicsInteraction((GroundedWarrirorAIPhysicsInteractionType.EnemyTriggerExit2D, collider, null));

	[ContextMenu(nameof(LogTargetCount))]
	public void LogTargetCount()
	{
		Debug.LogFormat("Target In Range Count: {0}", targetInRangeSet.Count);
	}

	[ContextMenu(nameof(LogPhysicsInteractionCount))]
	public void LogPhysicsInteractionCount()
	{
		Debug.LogFormat("Target In Range Count: {0}", groundedWarrirorAIPhysicsInteractionQueue.Count);
	}

	public override void CopyTo(in AIBase main)
	{
		if (main is GroundedWarrirorAIBase foundSelf)
		{
			foundSelf.goHomeBackTimer = this.goHomeBackTimer;

			foundSelf.singleNormalAttackDamage = this.singleNormalAttackDamage;
			foundSelf.singleNormalAttackBlockTimer = this.singleNormalAttackBlockTimer;
		}

		base.CopyTo(main);
	}


	// Dispose
	protected override void OnDisable()
	{
		DoFrameDependentPhysics();
		base.OnDisable();
	}
}


#if UNITY_EDITOR

public abstract partial class GroundedWarrirorAIBase
{ }

#endif