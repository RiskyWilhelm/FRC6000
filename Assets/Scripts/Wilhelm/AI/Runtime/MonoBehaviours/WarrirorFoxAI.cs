using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public partial class WarrirorFoxAI : GroundedAIBase, IHomeAccesser, IFrameDependentPhysicsInteractor<WarrirorFoxAITriggerType>
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
	private readonly HashSet<ITarget> targetInRangeSet = new();

	[NonSerialized]
	private readonly HashSet<ITarget> runawayTargetsInRangeSet = new();


	#endregion

	#region WarrirorFoxAI Other

	[field: NonSerialized]
	public bool IsCaughtMeal {  get; private set; }

	[field: NonSerialized]
	public bool OpenAIHomeGate { get; private set; }

	[field: NonSerialized]
	public HomeBase ParentHome { get; set; }

	[NonSerialized]
	private readonly Queue<(WarrirorFoxAITriggerType triggerType, Collider2D collider2D, Collision2D collision2D)> PhysicsInteractionQueue = new();


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

		// If it currently chasing something, do not go home and protect your family at what it costs!
		if (targetInRangeSet.Count > 0)
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

	public void RegisterToNearTargets(ITarget target)
	{
		if (acceptedTargetTypeList.Contains(target.TargetTag))
			targetInRangeSet.Add(target);

		if (runawayTargetTypeList.Contains(target.TargetTag))
			runawayTargetsInRangeSet.Add(target);
	}

	public void UnRegisterFromNearTargets(ITarget target)
	{
		targetInRangeSet.Remove(target);
		runawayTargetsInRangeSet.Remove(target);
	}

	public void RegisterFrameDependentPhysicsInteraction((WarrirorFoxAITriggerType triggerType, Collider2D collider2D, Collision2D collision2D) interaction)
	{
		if (!PhysicsInteractionQueue.Contains(interaction))
			PhysicsInteractionQueue.Enqueue(interaction);
	}

	public bool TrySetDestinationToHome()
	{
		if (ParentHome != null)
			return OpenAIHomeGate = TrySetDestinationTo(ParentHome.transform, 0.1f);

		return false;
	}

	public bool TrySetDestinationAwayFromOrToNearestTarget()
	{
		// Let the blocked states take control
		if (State is PlayerStateType.Jumping or PlayerStateType.Attacking or PlayerStateType.Defending or PlayerStateType.Dead)
			return false;

		// If there is any powerful enemy in range, runaway from it and discard other targets
		if ((runawayTargetsInRangeSet.Count > 0) && TrySetDestinationAwayFromNearestIn(runawayTargetsInRangeSet))
		{
			State = PlayerStateType.Running;
			return true;
		}

		// Try catch the nearest enemy
		if (!IsCaughtMeal && TrySetDestinationToNearestIn(targetInRangeSet))
		{
			State = PlayerStateType.Running;
			return true;
		}

		return false;
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
			RefreshAttackState();
			return true;
		}

		return false;
	}

	private void RefreshAttackState()
	{
		if (State == PlayerStateType.Attacking)
		{
			if (currentSingleNormalAttack.coroutine != null)
				StopCoroutine(currentSingleNormalAttack.coroutine);

			State = PlayerStateType.Idle;
		}

		currentSingleNormalAttack = default;
		singleNormalAttackTimer.ResetAndRandomize();
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
			RefreshAttackState();
			OnKilledTarget(target);
			yield break;
		}

		// Lock the state to Attacking until timer ends
		while (!singleNormalAttackTimer.Tick())
			yield return null;

		RefreshAttackState();
	}

	public void DoFrameDependentPhysics()
	{
		for (int i = PhysicsInteractionQueue.Count - 1; i >= 0; i--)
		{
			var iteratedPhysicsInteraction = PhysicsInteractionQueue.Dequeue();

			switch (iteratedPhysicsInteraction.triggerType)
			{
				case WarrirorFoxAITriggerType.EnemyTriggerStay:
				{
					if (EventReflectorUtils.TryGetComponentByEventReflector<ITarget>(iteratedPhysicsInteraction.collider2D.gameObject, out ITarget foundTarget))
					{
						RegisterToNearTargets(foundTarget);
						TrySetDestinationAwayFromOrToNearestTarget();
					}
				}
				break;

				case WarrirorFoxAITriggerType.EnemyTriggerExit:
				{
					if (EventReflectorUtils.TryGetComponentByEventReflector<ITarget>(iteratedPhysicsInteraction.collider2D.gameObject, out ITarget foundTarget))
						UnRegisterFromNearTargets(foundTarget);
				}
				break;

				case WarrirorFoxAITriggerType.SingleNormalAttackTriggerStay:
				{
					if (EventReflectorUtils.TryGetComponentByEventReflector<ITarget>(iteratedPhysicsInteraction.collider2D.gameObject, out ITarget foundTarget))
						TryDoSingleNormalAttackTo(foundTarget);
				}
				break;

				case WarrirorFoxAITriggerType.SingleNormalAttackTriggerExit:
				{
					if (EventReflectorUtils.TryGetComponentByEventReflector<ITarget>(iteratedPhysicsInteraction.collider2D.gameObject, out ITarget escapedTarget))
						TryCancelSingleNormalAttackFrom(escapedTarget);
				}
				break;
			}
		}
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
		GameControllerSingleton.Instance.onTargetDeathDict[TargetType.WarrirorFox]?.Invoke();
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
		=> RegisterFrameDependentPhysicsInteraction((WarrirorFoxAITriggerType.SingleNormalAttackTriggerStay, collider, null));

	public void OnSingleNormalAttackTriggerExit2D(Collider2D collider)
		=> RegisterFrameDependentPhysicsInteraction((WarrirorFoxAITriggerType.SingleNormalAttackTriggerExit, collider, null));

	public void OnEnemyTriggerStay2D(Collider2D collider)
		=> RegisterFrameDependentPhysicsInteraction((WarrirorFoxAITriggerType.EnemyTriggerStay, collider, null));

	public void OnEnemyTriggerExit2D(Collider2D collider)
		=> RegisterFrameDependentPhysicsInteraction((WarrirorFoxAITriggerType.EnemyTriggerExit, collider, null));

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
		Debug.LogFormat("Target In Range Count: {0}", PhysicsInteractionQueue.Count);
	}


	// Dispose
	private void OnDisable()
	{
		if (GameControllerSingleton.IsQuitting)
			return;

		DoFrameDependentPhysics();
	}

	private void OnDestroy()
	{
		if (GameControllerSingleton.IsQuitting)
			return;

		DayCycleControllerSingleton.Instance.onDaylightTypeChanged.RemoveListener(OnDaylightTypeChanged);
	}
}


#if UNITY_EDITOR

public partial class WarrirorFoxAI
{ }

#endif