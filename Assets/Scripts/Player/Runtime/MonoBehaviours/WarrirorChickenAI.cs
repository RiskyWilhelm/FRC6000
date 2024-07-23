using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public partial class WarrirorChickenAI : GroundedWarrirorAIBase
{
	[Header("WarrirorChickenAI Visuals")]
	#region WarrirorChickenAI Visuals

	[SerializeField]
	private Animator animator;


	#endregion

	#region WarrirorFoxAI Other

	[field: NonSerialized]
	public bool IsForcedToGoHome { get; private set; }


	#endregion


	// Update
	public override void TakeDamage(uint damage, Vector2 occuredWorldPosition)
	{
		animator.Play("Damaged");
		base.TakeDamage(damage, occuredWorldPosition);
	}

	public override void ForceToGoHome()
	{
		IsForcedToGoHome = true;
		base.ForceToGoHome();
	}

	public override bool TrySetDestinationAwayFromOrToNearestTarget()
	{
		return TrySetDestinationAwayFromNearestTarget() || (!IsForcedToGoHome && TrySetDestinationToNearestTarget());
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
		if (goHomeBackTimer.HasEnded || IsForcedToGoHome)
		{
			if (TrySetDestinationToHome())
			{
				if (IsForcedToGoHome)
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
		animator.Play("Idle");
		base.OnStateChangedToIdle();
	}

	protected override void OnStateChangedToWalking()
	{
		animator.Play("Walking");
		base.OnStateChangedToWalking();
	}

	protected override void OnStateChangedToRunning()
	{
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
		PlayerControllerSingleton.onTargetDeathEventDict[TargetType.WarrirorChicken]?.Invoke();
		base.OnStateChangedToDead();
	}

	public override void OnEnteredAIHome(HomeBase home)
	{
		IsForcedToGoHome = false;
		base.OnEnteredAIHome(home);
	}

	// Dispose
	protected override void OnDisable()
	{
		DoFrameDependentPhysics();
		IsForcedToGoHome = false;
		base.OnDisable();
	}
}


#if UNITY_EDITOR

public partial class WarrirorChickenAI
{ }

#endif