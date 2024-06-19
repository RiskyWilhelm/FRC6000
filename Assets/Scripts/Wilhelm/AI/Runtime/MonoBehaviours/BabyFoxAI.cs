using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class BabyFoxAI : GroundedAIBase, IAIHomeAccesser
{
	#region BabyFoxAI Movement

	[SerializeField]
	private Timer goHomeBackTimer = new(10f, 10f, 20f);

	private bool mustGoHomeBack;


	#endregion

	[Header("BabyFoxAI Normal Attack")]
	#region BabyFoxAI Normal Attack

	[SerializeField]
	private uint normalAttackDamage = 1;

	[SerializeField]
	private Timer normalAttackTimer = new(0.5f, 0.75f);

	[NonSerialized]
	private ValueTuple<ITarget, Coroutine> currentNormalAttack;

	#endregion

	[Header("BabyFoxAI Enemy")]
	#region BabyFoxAI Enemy

	[NonSerialized]
	private readonly HashSet<ITarget> targetInRangeSet = new();

	[NonSerialized]
	private readonly HashSet<ITarget> runawayTargetsInRangeSet = new();

	#endregion

	#region BabyFoxAI Other

	[field: NonSerialized]
	public bool IsCaughtMeal {  get; private set; }

	[field: SerializeField]
	public bool OpenAIHomeGate { get; private set; }

	[field: NonSerialized]
	public AIHomeBase ParentHome { get; set; }

	#endregion


	// Initialize
	protected override void OnEnable()
	{
		mustGoHomeBack = false;
		goHomeBackTimer.Reset();
		ClearDestination();
		RefreshAttackState();

		base.OnEnable();
	}

	private void Start()
	{
		DayCycleControllerSingleton.Instance.onDaylightTypeChanged.AddListener(OnDaylightTypeChanged);
		UpdateByDaylightType(DayCycleControllerSingleton.Instance.Time.daylightType);
	}


	// Update
	protected override void DoIdle()
	{
		bool isGoingHome = false;

		if (mustGoHomeBack || IsCaughtMeal)
			isGoingHome = TrySetDestinationToHome();
		
		if (!isGoingHome)
			base.DoIdle();
	}

	protected override void Update()
	{
		if (goHomeBackTimer.Tick())
			mustGoHomeBack = true;

		base.Update();
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

	public void OnEnteredAIHome(AIHomeBase home)
	{
		IsCaughtMeal = false;
		ReleaseOrDestroySelf();
	}

	public void OnLeftFromAIHome(AIHomeBase home)
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
		if (target.TargetTag is TargetType.Chicken or TargetType.ChickenHome)
			IsCaughtMeal = true;

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
		normalAttackTimer.Reset();
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
			if (State is PlayerStateType.Jumping or PlayerStateType.Attacking or PlayerStateType.Dead)
				return;

			// If there is any powerful enemy in range, runaway from it and discard other targets
			if ((runawayTargetsInRangeSet.Count > 0) && TrySetDestinationAwayFromNearestIn(runawayTargetsInRangeSet))
				return;

			// If didnt caught any meal, try catch the nearest enemy
			if (!IsCaughtMeal)
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
		if (main is BabyFoxAI babyFoxAI)
		{
			babyFoxAI.normalAttackTimer = this.normalAttackTimer;
			babyFoxAI.normalAttackDamage = this.normalAttackDamage;
			babyFoxAI.goHomeBackTimer = this.goHomeBackTimer;
		}

		base.CopyTo(main);
	}


	// Dispose
	private void OnDestroy()
	{
		if (AppStateControllerSingleton.IsQuitting)
			return;

		DayCycleControllerSingleton.Instance.onDaylightTypeChanged.RemoveListener(OnDaylightTypeChanged);
	}
}


#if UNITY_EDITOR

public partial class BabyFoxAI
{ }

#endif