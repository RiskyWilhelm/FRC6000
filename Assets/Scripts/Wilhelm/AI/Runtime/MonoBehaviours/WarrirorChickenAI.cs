using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class WarrirorChickenAI : GroundedAIBase, IHomeAccesser
{
	[Header("WarrirorChickenAI Movement")]
	#region WarrirorChickenAI Movement

	[SerializeField]
	private TimerRandomized goHomeBackTimer = new(10f, 10f, 20f);


	#endregion

	[Header("WarrirorChickenAI Normal Attack")]
	#region WarrirorChickenAI Normal Attack

	[SerializeField]
	private uint normalAttackDamage = 1;

	[SerializeField]
	private TimerRandomized normalAttackTimer = new(0.5f, 0.75f);

	[NonSerialized]
	private ValueTuple<ITarget, Coroutine> currentNormalAttack;

	#endregion

	[Header("WarrirorChickenAI Enemy")]
	#region WarrirorChickenAI Enemy

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

		base.Update();
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
			else
				goHomeBackTimer.ResetAndRandomize();
		}

		// Otherwise, continue old idle
		base.DoIdle();
	}

	protected override void OnStateChangedToDead()
	{
		GameControllerSingleton.Instance.onFoxDeath?.Invoke();
		ReleaseOrDestroySelf();
		base.OnStateChangedToDead();
	}

	protected override void OnChangedDestination(Vector2? newDestination)
	{
		OpenAIHomeGate = false;
		base.OnChangedDestination(newDestination);
	}

	public bool TrySetDestinationToHome()
	{
		var isDestinationSet = false;

		if (ParentHome != null)
		{
			isDestinationSet = TrySetDestinationTo(ParentHome.transform, 0.1f);

			if (isDestinationSet)
				OpenAIHomeGate = true;
		}

		return isDestinationSet;
	}

	public void OnEnteredAIHome(HomeBase home)
	{
		ReleaseOrDestroySelf();
	}

	public void OnLeftFromAIHome(HomeBase home)
	{
		OpenAIHomeGate = false;
	}

	/// <summary> Attacks single target </summary>
	private IEnumerator DoNormalAttack(ITarget target)
	{
		// Cant attack the dead
		if (target.IsDead)
			yield break;

		// Take full control over the body by setting state to attacking, ready timer
		State = PlayerStateType.Attacking;

		// Do the attack
		target.TakeDamage(normalAttackDamage, selfRigidbody.position);

		if (target.IsDead)
		{
			RefreshAttackState();
			OnKilledTarget(target);
			yield break;
		}

		// Lock the state to Attacking until timer ends
		while (!normalAttackTimer.Tick())
			yield return null;

		RefreshAttackState();
	}

	private void OnKilledTarget(ITarget target)
	{
		TrySetDestinationToHome();
	}

	private void RefreshAttackState()
	{
		if (State == PlayerStateType.Attacking)
		{
			if (currentNormalAttack.Item2 != null)
				StopCoroutine(currentNormalAttack.Item2);

			State = PlayerStateType.Idle;
		}

		currentNormalAttack = default;
		normalAttackTimer.ResetAndRandomize();
	}

	public void OnNormalAttackTriggerStay2D(Collider2D collider)
	{
		// If attacking, let it finish first
		if (State == PlayerStateType.Attacking)
			return;

		// Do the attack
		if (EventReflectorUtils.TryGetComponentByEventReflector<ITarget>(collider.gameObject, out ITarget foundTarget))
		{
			// If target is not accepted, return
			if (!acceptedTargetTypeList.Contains(foundTarget.TargetTag))
				return;

			currentNormalAttack.Item1 = foundTarget;
			currentNormalAttack.Item2 = StartCoroutine(DoNormalAttack(foundTarget));
		}
	}

	public void OnNormalAttackTriggerExit2D(Collider2D collider)
	{
		if (EventReflectorUtils.TryGetComponentByEventReflector<ITarget>(collider.gameObject, out ITarget escapedTarget))
		{
			if (escapedTarget == currentNormalAttack.Item1)
				RefreshAttackState();
		}
	}

	public void OnEnemyTriggerStay2D(Collider2D collider)
	{
		// If the GameObject is a AI Target, add to range list
		if (EventReflectorUtils.TryGetComponentByEventReflector<ITarget>(collider.gameObject, out ITarget foundTarget))
		{
			// Update enemies in range
			if (acceptedTargetTypeList.Contains(foundTarget.TargetTag))
				targetInRangeSet.Add(foundTarget);

			if (runawayTargetTypeList.Contains(foundTarget.TargetTag))
				runawayTargetsInRangeSet.Add(foundTarget);

			// Let the blocked states take control
			if (State is PlayerStateType.Jumping or PlayerStateType.Attacking or PlayerStateType.Defending or PlayerStateType.Dead)
				return;

			// If there is any powerful enemy in range, runaway from it and discard other targets
			if ((runawayTargetsInRangeSet.Count > 0) && TrySetDestinationAwayFromNearestIn(runawayTargetsInRangeSet))
				return;

			// Try catch the nearest enemy
			TrySetDestinationToNearestIn(targetInRangeSet);
		}
	}

	public void OnEnemyTriggerExit2D(Collider2D collider)
	{
		// If the GameObject is a AI Target, remove from the range list
		if (EventReflectorUtils.TryGetComponentByEventReflector<ITarget>(collider.gameObject, out ITarget foundTarget))
		{
			runawayTargetsInRangeSet.Remove(foundTarget);
			targetInRangeSet.Remove(foundTarget);
		}
	}

	public override void CopyTo(in AIBase main)
	{
		if (main is WarrirorChickenAI foundSelf)
		{
			foundSelf.normalAttackTimer = this.normalAttackTimer;
			foundSelf.normalAttackDamage = this.normalAttackDamage;
			foundSelf.goHomeBackTimer = this.goHomeBackTimer;
		}

		base.CopyTo(main);
	}
}


#if UNITY_EDITOR

public partial class WarrirorChickenAI
{ }

#endif