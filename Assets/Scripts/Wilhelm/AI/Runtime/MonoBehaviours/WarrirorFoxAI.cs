using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class WarrirorFoxAI : GroundedAIBase, IHomeAccesser
{
	[Header("WarrirorFoxAI Movement")]
	#region BabyFoxAI Movement

	[SerializeField]
	private TimerRandomized goHomeBackTimer = new(10f, 10f, 20f);


	#endregion

	[Header("WarrirorFoxAI Normal Attack")]
	#region WarrirorFoxAI Normal Attack

	[SerializeField]
	private uint normalAttackDamage = 1;

	[SerializeField]
	private TimerRandomized normalAttackTimer = new(0.5f, 0.75f);

	[NonSerialized]
	private ValueTuple<ITarget, Coroutine> currentNormalAttack;

	#endregion

	[Header("WarrirorFoxAI Enemy")]
	#region WarrirorFoxAI Enemy

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

		// If wants to go home, set state to walking or running depending on the meal state
		if (goHomeBackTimer.HasEnded || IsCaughtMeal)
		{
			if (TrySetDestinationToHome())
			{
				if (IsCaughtMeal)
					State = PlayerStateType.Running;
				else
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
		IsCaughtMeal = false;
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
		// If target is a chicken or chicken home, it means it caught a chicken
		if (target.TargetTag is TargetType.BabyChicken or TargetType.ChickenHome)
		{
			IsCaughtMeal = true;
			TrySetDestinationToHome();
		}
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
		if (EventReflectorUtils.TryGetComponentByEventReflector<ITarget>(collider.gameObject, out ITarget foundTarget))
		{
			// Update enemies in range
			if (acceptedTargetTypeList.Contains(foundTarget.TargetTag))
				targetInRangeSet.Add(foundTarget);
			else
				targetInRangeSet.Remove(foundTarget);

			if (runawayTargetTypeList.Contains(foundTarget.TargetTag))
				runawayTargetsInRangeSet.Add(foundTarget);
			else
				runawayTargetsInRangeSet.Remove(foundTarget);

			// Let the blocked states take control
			if (State is PlayerStateType.Jumping or PlayerStateType.Attacking or PlayerStateType.Defending or PlayerStateType.Dead)
				return;

			// If there is any powerful enemy in range, runaway from it and discard other targets
			if ((runawayTargetsInRangeSet.Count > 0) && TrySetDestinationAwayFromNearestIn(runawayTargetsInRangeSet))
			{
				State = PlayerStateType.Running;
				return;
			}

			// If didnt caught any meal, try catch the nearest enemy
			if (!IsCaughtMeal)
			{
				if (TrySetDestinationToNearestIn(targetInRangeSet))
					State = PlayerStateType.Running;
			}
		}
	}

	public void OnEnemyTriggerExit2D(Collider2D collider)
	{
		if (EventReflectorUtils.TryGetComponentByEventReflector<ITarget>(collider.gameObject, out ITarget foundTarget))
		{
			// Update enemies in range
			targetInRangeSet.Remove(foundTarget);
			runawayTargetsInRangeSet.Remove(foundTarget);
		}
	}

	private void OnDaylightTypeChanged(DaylightType newDaylightType)
	{
		UpdateByDaylightType(newDaylightType);
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

	public override void CopyTo(in AIBase main)
	{
		if (main is WarrirorFoxAI foundSelf)
		{
			// Movement
			foundSelf.goHomeBackTimer = this.goHomeBackTimer;

			// Normal Attack
			foundSelf.normalAttackTimer = this.normalAttackTimer;
			foundSelf.normalAttackDamage = this.normalAttackDamage;
		}

		base.CopyTo(main);
	}


	// Dispose
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