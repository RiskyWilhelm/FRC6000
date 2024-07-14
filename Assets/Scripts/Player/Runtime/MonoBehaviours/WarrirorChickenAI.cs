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


	// Update
	public override void TakeDamage(uint damage, Vector2 occuredWorldPosition)
	{
		animator.Play("Damaged");
		base.TakeDamage(damage, occuredWorldPosition);
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