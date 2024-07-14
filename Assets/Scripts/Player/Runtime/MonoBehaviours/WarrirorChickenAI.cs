using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public partial class WarrirorChickenAI : GroundedAIBase, IHomeAccesser, IFrameDependentPhysicsInteractor<WarrirorChickenAIPhysicsInteractionType>
{
	[Header("WarrirorChickenAI Movement")]
	#region WarrirorChickenAI Movement

	[SerializeField]
	private TimerRandomized goHomeBackTimer = new(10f, 10f, 20f);


	#endregion

	[Header("WarrirorChickenAI Single Normal Attack")]
	#region WarrirorChickenAI Single Normal Attack

	[SerializeField]
	private uint singleNormalAttackDamage = 1;

	[SerializeField]
	[Tooltip("Sticks the state")]
	private Timer singleNormalAttackBlockTimer;

	[NonSerialized]
	private ITargetValue<Coroutine> currentSingleNormalAttack;

	public bool IsSingleNormalAttacking => (currentSingleNormalAttack != default);


	#endregion

	[Header("BabyChickenAI Visuals")]
	#region BabyChickenAI Visuals

	[SerializeField]
	private Animator animator;


	#endregion

	#region WarrirorChickenAI Target

	[NonSerialized]
	private readonly HashSet<ITargetValue<Transform>> targetInRangeSet = new();


	#endregion

	#region WarrirorChickenAI Other

	[field: NonSerialized]
	public bool OpenAIHomeGate { get; private set; }

	[field: NonSerialized]
	public HomeBase ParentHome { get; set; }

	[NonSerialized]
	private readonly Queue<(WarrirorChickenAIPhysicsInteractionType triggerType, Collider2D collider2D, Collision2D collision2D)> PhysicsInteractionQueue = new();


	#endregion


	// Initialize
	protected override void OnEnable()
	{
		goHomeBackTimer.ResetAndRandomize();
		RefreshAttackState();

		base.OnEnable();
	}


	// Update
	protected override void Update()
	{
		goHomeBackTimer.Tick();

		if (IsSingleNormalAttacking)
            goHomeBackTimer.Reset();

		DoFrameDependentPhysics();
		base.Update();
	}

	public void RegisterFrameDependentPhysicsInteraction((WarrirorChickenAIPhysicsInteractionType triggerType, Collider2D collider2D, Collision2D collision2D) interaction)
	{
		if (!PhysicsInteractionQueue.Contains(interaction))
			PhysicsInteractionQueue.Enqueue(interaction);
	}

	public override void TakeDamage(uint damage, Vector2 occuredWorldPosition)
	{
		animator.Play("Damaged");
		base.TakeDamage(damage, occuredWorldPosition);
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
			isSetDestination = TrySetDestinationAwayFromVector(nearestTransform.position, isGroundedOnly: true);
		}

		// If there is any accepted target and cant runaway, catch the nearest target
		if (!isSetDestination && (cachedAcceptedTransformList.Count > 0))
		{
			this.transform.TryGetNearestTransform(cachedAcceptedTransformList.GetEnumerator(), out Transform nearestTransform);
			isSetDestination = TrySetDestinationToVector(nearestTransform.position);
		}

		if (isSetDestination && !IsSingleNormalAttacking && (State is not PlayerStateType.Jumping or PlayerStateType.Flying or PlayerStateType.Attacking or PlayerStateType.Blocked or PlayerStateType.Dead))
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
		if (currentSingleNormalAttack.value != null)
			StopCoroutine(currentSingleNormalAttack.value);

		if (State is PlayerStateType.Attacking)
			State = PlayerStateType.Idle;

		currentSingleNormalAttack = default;
		singleNormalAttackBlockTimer.Reset();
	}

	private void RefreshAttackState()
	{
		RefreshSingleNormalAttack();
	}

	private IEnumerator DoSingleNormalAttack(ITarget target)
	{
		// Take full control over the body by setting state to attacking, ready timer
		State = PlayerStateType.Attacking;

		// Lock the state to Attacking until timer ends
		while (!singleNormalAttackBlockTimer.Tick())
			yield return null;

		// Target is killed by unknown thing while attacking
		if (target.IsDead)
		{
			RefreshSingleNormalAttack();
			yield break;
		}

		// Do the attack
		target.TakeDamage(singleNormalAttackDamage, SelfRigidbody.position);

		if (target.IsDead)
			OnKilledTarget(target);

		RefreshSingleNormalAttack();
	}

	public void DoFrameDependentPhysics()
	{
		for (int i = PhysicsInteractionQueue.Count - 1; i >= 0; i--)
		{
			var iteratedPhysicsInteraction = PhysicsInteractionQueue.Dequeue();

			switch (iteratedPhysicsInteraction.triggerType)
			{
				case WarrirorChickenAIPhysicsInteractionType.EnemyTriggerEnter2D:
				DoEnemyTriggerEnter2D(iteratedPhysicsInteraction);
				break;

				case WarrirorChickenAIPhysicsInteractionType.EnemyTriggerStay2D:
				DoEnemyTriggerStay2D(iteratedPhysicsInteraction);
				break;

				case WarrirorChickenAIPhysicsInteractionType.EnemyTriggerExit2D:
				DoEnemyTriggerExit2D(iteratedPhysicsInteraction);
				break;

				case WarrirorChickenAIPhysicsInteractionType.SingleNormalAttackTriggerStay2D:
				DoSingleNormalAttackTriggerStay2D(iteratedPhysicsInteraction);
				break;

				case WarrirorChickenAIPhysicsInteractionType.SingleNormalAttackTriggerExit2D:
				DoSingleNormalAttackTriggerExit2D(iteratedPhysicsInteraction);
				break;
			}
		}
	}

	private void DoEnemyTriggerEnter2D((WarrirorChickenAIPhysicsInteractionType triggerType, Collider2D collider2D, Collision2D collision2D) interaction)
	{
		if (!interaction.collider2D)
			return;

		if (EventReflectorUtils.TryGetComponentByEventReflector<ITarget>(interaction.collider2D.gameObject, out ITarget foundTarget))
			targetInRangeSet.Add(new ITargetValue<Transform>(foundTarget, (foundTarget as Component).transform));
	}

	private void DoEnemyTriggerStay2D((WarrirorChickenAIPhysicsInteractionType triggerType, Collider2D collider2D, Collision2D collision2D) interaction)
	{
		if (!interaction.collider2D)
			return;

		TrySetDestinationAwayFromOrToNearestTarget();
	}

	private void DoEnemyTriggerExit2D((WarrirorChickenAIPhysicsInteractionType triggerType, Collider2D collider2D, Collision2D collision2D) interaction)
	{
		if (!interaction.collider2D)
		{
			targetInRangeSet.RemoveWhere((iteratedTargetTuple) => !iteratedTargetTuple.value);
			return;
		}

		if (EventReflectorUtils.TryGetComponentByEventReflector<ITarget>(interaction.collider2D.gameObject, out ITarget foundTarget))
			targetInRangeSet.Remove(new ITargetValue<Transform>(foundTarget, (foundTarget as Component).transform));
	}

	private void DoSingleNormalAttackTriggerStay2D((WarrirorChickenAIPhysicsInteractionType triggerType, Collider2D collider2D, Collision2D collision2D) interaction)
	{
		if (!interaction.collider2D)
			return;

		if (!IsSingleNormalAttacking && EventReflectorUtils.TryGetComponentByEventReflector<ITarget>(interaction.collider2D.gameObject, out ITarget foundTarget))
			TryDoSingleNormalAttackTo(foundTarget);
	}

	private void DoSingleNormalAttackTriggerExit2D((WarrirorChickenAIPhysicsInteractionType triggerType, Collider2D collider2D, Collision2D collision2D) interaction)
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

		// If wants to go home, set state to walking
		if (goHomeBackTimer.HasEnded)
		{
			goHomeBackTimer.ResetAndRandomize();

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

	protected override void OnStateChangedToIdle()
	{
		animator.Play("Idle");
	}

	protected override void OnStateChangedToWalking()
	{
		animator.Play("Walking");
	}

	protected override void OnStateChangedToRunning()
	{
		animator.Play("Running");
	}

	protected override void OnStateChangedToJumping()
	{
		animator.Play("Jumping");
	}

	protected override void OnStateChangedToAttacking()
	{
		animator.Play("Attacking");
	}

	protected override void OnStateChangedToDead()
	{
		PlayerControllerSingleton.onTargetDeathEventDict[TargetType.WarrirorChicken]?.Invoke();
		ReleaseOrDestroySelf();
	}

	protected override void OnStateChangedToAny(PlayerStateType newState)
	{
		if (newState is not PlayerStateType.Attacking)
			RefreshAttackState();
	}

	protected override void OnChangedDestination(Vector2? newDestination)
	{
		OpenAIHomeGate = false;
		base.OnChangedDestination(newDestination);
	}

	private void OnKilledTarget(ITarget target)
	{
		TrySetDestinationToHome();
	}

	public void OnEnteredAIHome(HomeBase home)
	{
		ReleaseOrDestroySelf();
	}

	public void OnLeftFromAIHome(HomeBase home)
	{
		OpenAIHomeGate = false;
	}

	public void OnSingleNormalAttackTriggerStay2D(Collider2D collider)
		=> RegisterFrameDependentPhysicsInteraction((WarrirorChickenAIPhysicsInteractionType.SingleNormalAttackTriggerStay2D, collider, null));

	public void OnSingleNormalAttackTriggerExit2D(Collider2D collider)
		=> RegisterFrameDependentPhysicsInteraction((WarrirorChickenAIPhysicsInteractionType.SingleNormalAttackTriggerExit2D, collider, null));

	public void OnEnemyTriggerEnter2D(Collider2D collider)
		=> RegisterFrameDependentPhysicsInteraction((WarrirorChickenAIPhysicsInteractionType.EnemyTriggerEnter2D, collider, null));

	public void OnEnemyTriggerStay2D(Collider2D collider)
		=> RegisterFrameDependentPhysicsInteraction((WarrirorChickenAIPhysicsInteractionType.EnemyTriggerStay2D, collider, null));

	public void OnEnemyTriggerExit2D(Collider2D collider)
		=> RegisterFrameDependentPhysicsInteraction((WarrirorChickenAIPhysicsInteractionType.EnemyTriggerExit2D, collider, null));

	[ContextMenu(nameof(LogTargetCount))]
	public void LogTargetCount()
	{
		Debug.LogFormat("Target In Range Count: {0}", targetInRangeSet.Count);
	}

	[ContextMenu(nameof(LogPhysicsInteractionCount))]
	public void LogPhysicsInteractionCount()
	{
		Debug.LogFormat("Target In Range Count: {0}", PhysicsInteractionQueue.Count);
	}

	public override void CopyTo(in AIBase main)
	{
		if (main is WarrirorChickenAI foundSelf)
		{
			foundSelf.singleNormalAttackBlockTimer = this.singleNormalAttackBlockTimer;
			foundSelf.singleNormalAttackDamage = this.singleNormalAttackDamage;
			foundSelf.goHomeBackTimer = this.goHomeBackTimer;
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

public partial class WarrirorChickenAI
{ }

#endif