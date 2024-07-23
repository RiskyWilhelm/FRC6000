using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public sealed partial class WarrirorFoxAI : GroundedWarrirorAIBase
{
	[Header("BabyChickenAI Visuals")]
	#region BabyChickenAI Visuals

	[SerializeField]
	private Animator animator;


	#endregion

	#region WarrirorFoxAI Other

	[field: NonSerialized]
	public bool IsCaughtMeal { get; private set; }


	#endregion


	// Initialize
	private void Start()
	{
		DayCycleControllerSingleton.Instance.onDaylightTypeChanged.AddListener(OnDaylightTypeChanged);
		UpdateByDaylightType(DayCycleControllerSingleton.Instance.GameTimeDaylightType);
	}


	// Update
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

			default:
			goto case DaylightType.Light;
		}
	}

	public override void TakeDamage(uint damage, Vector2 occuredWorldPosition)
	{
		IsCaughtMeal = false;
		goHomeBackTimer.CurrentSecond = Mathf.Clamp(goHomeBackTimer.CurrentSecond + 5f, 0f, goHomeBackTimer.TickSecond);
		base.TakeDamage(damage, occuredWorldPosition);
	}

	public override void ForceToGoHome()
	{
		IsCaughtMeal = true;
		base.ForceToGoHome();
	}

	public override bool TrySetDestinationAwayFromOrToNearestTarget()
	{
		return TrySetDestinationAwayFromNearestTarget() || (!IsCaughtMeal && TrySetDestinationToNearestTarget());
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
		}

		// Otherwise, continue old idle
		base.DoIdle();
	}

	protected override void OnStateChangedToIdle()
	{
		Debug.Log("OnStateChangedToIdle");
		animator.Play("Idle");
		base.OnStateChangedToIdle();
	}

	protected override void OnStateChangedToWalking()
	{
		Debug.Log("OnStateChangedToWalking");
		animator.Play("Walking");
		base.OnStateChangedToWalking();
	}

	protected override void OnStateChangedToRunning()
	{
		Debug.Log("OnStateChangedToRunning");
		animator.Play("Running");
		base.OnStateChangedToRunning();
	}

	protected override void OnStateChangedToJumping()
	{
		animator.Play("Jumping");
		base.OnStateChangedToJumping();
	}

	protected override void OnStateChangedToAttacking()
	{
		animator.Play("Attacking");
		base.OnStateChangedToAttacking();
	}

	protected override void OnStateChangedToDead()
	{
		PlayerControllerSingleton.onTargetDeathEventDict[TargetType.WarrirorFox]?.Invoke();
		base.OnStateChangedToDead();
	}

	private void OnDaylightTypeChanged(DaylightType newDaylightType)
		=> UpdateByDaylightType(newDaylightType);

	protected override void OnKilledTarget(ITarget target)
	{
		// If target is a chicken or chicken home, it means it caught a chicken
		if (target.TargetTag is TargetType.BabyChicken or TargetType.ChickenHome)
		{
			IsCaughtMeal = true;
			TrySetDestinationToHome();
		}
	}

	public override void OnEnteredAIHome(HomeBase home)
	{
		IsCaughtMeal = false;
		base.OnEnteredAIHome(home);
	}

	// Dispose
	protected override void OnDisable()
	{
		IsCaughtMeal = false;
		base.OnDisable();
	}

	private void OnDestroy()
	{
		DayCycleControllerSingleton.Instance?.onDaylightTypeChanged.RemoveListener(OnDaylightTypeChanged);
	}
}


#if UNITY_EDITOR

public sealed partial class WarrirorFoxAI
{ }

#endif