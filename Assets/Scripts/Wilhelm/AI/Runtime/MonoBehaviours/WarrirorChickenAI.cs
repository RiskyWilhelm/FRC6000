using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public partial class WarrirorChickenAI : GroundedAIBase, IHomeAccesser, IFrameDependentPhysicsInteractor<WarrirorChickenAITriggerType>
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
	private TimerRandomized singleNormalAttackTimer = new(0.5f, 0.75f);

	[NonSerialized]
	private (ITarget target, Coroutine coroutine) currentSingleNormalAttack;


	#endregion

	#region WarrirorChickenAI Target

	[NonSerialized]
	private readonly HashSet<ITarget> targetInRangeSet = new();

	[NonSerialized]
	private readonly HashSet<ITarget> runawayTargetsInRangeSet = new();


	#endregion

	#region WarrirorChickenAI Other

	[field: NonSerialized]
	public bool OpenAIHomeGate { get; private set; }

	[field: NonSerialized]
	public HomeBase ParentHome { get; set; }

	[NonSerialized]
	private readonly Queue<(WarrirorChickenAITriggerType triggerType, Collider2D collider2D, Collision2D collision2D)> PhysicsInteractionQueue = new();


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

		// If it currently chasing something, do not go home and protect your family at what it costs!
		if (targetInRangeSet.Count > 0)
            goHomeBackTimer.Reset();

		DoFrameDependentPhysics();
		base.Update();
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

	public void RegisterFrameDependentPhysicsInteraction((WarrirorChickenAITriggerType triggerType, Collider2D collider2D, Collision2D collision2D) interaction)
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
		if (TrySetDestinationToNearestIn(targetInRangeSet))
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
				case WarrirorChickenAITriggerType.EnemyTriggerStay:
				{
					if (EventReflectorUtils.TryGetComponentByEventReflector<ITarget>(iteratedPhysicsInteraction.collider2D.gameObject, out ITarget foundTarget))
					{
						RegisterToNearTargets(foundTarget);
						TrySetDestinationAwayFromOrToNearestTarget();
					}
				}
				break;

				case WarrirorChickenAITriggerType.EnemyTriggerExit:
				{
					if (EventReflectorUtils.TryGetComponentByEventReflector<ITarget>(iteratedPhysicsInteraction.collider2D.gameObject, out ITarget foundTarget))
						UnRegisterFromNearTargets(foundTarget);
				}
				break;

				case WarrirorChickenAITriggerType.SingleNormalAttackTriggerStay:
				{
					if (EventReflectorUtils.TryGetComponentByEventReflector<ITarget>(iteratedPhysicsInteraction.collider2D.gameObject, out ITarget foundTarget))
						TryDoSingleNormalAttackTo(foundTarget);
				}
				break;

				case WarrirorChickenAITriggerType.SingleNormalAttackTriggerExit:
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

	protected override void OnStateChangedToDead()
	{
		GameControllerSingleton.Instance.onTargetDeathDict[TargetType.WarrirorChicken]?.Invoke();
		ReleaseOrDestroySelf();
		base.OnStateChangedToDead();
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
		=> RegisterFrameDependentPhysicsInteraction((WarrirorChickenAITriggerType.SingleNormalAttackTriggerStay, collider, null));

	public void OnSingleNormalAttackTriggerExit2D(Collider2D collider)
		=> RegisterFrameDependentPhysicsInteraction((WarrirorChickenAITriggerType.SingleNormalAttackTriggerExit, collider, null));

	public void OnEnemyTriggerStay2D(Collider2D collider)
		=> RegisterFrameDependentPhysicsInteraction((WarrirorChickenAITriggerType.EnemyTriggerStay, collider, null));

	public void OnEnemyTriggerExit2D(Collider2D collider)
		=> RegisterFrameDependentPhysicsInteraction((WarrirorChickenAITriggerType.EnemyTriggerExit, collider, null));

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
			foundSelf.singleNormalAttackTimer = this.singleNormalAttackTimer;
			foundSelf.singleNormalAttackDamage = this.singleNormalAttackDamage;
			foundSelf.goHomeBackTimer = this.goHomeBackTimer;
		}

		base.CopyTo(main);
	}


	// Dispose
	private void OnDisable()
	{
		if (GameControllerSingleton.IsQuitting)
			return;

		DoFrameDependentPhysics();
	}
}


#if UNITY_EDITOR

public partial class WarrirorChickenAI
{ }

#endif