using FMOD.Studio;
using FMODUnity;
using System;
using UnityEngine;

public partial class WarrirorChickenAI : GroundedWarrirorAIBase
{
	[Header("WarrirorChickenAI Visuals")]
	#region WarrirorChickenAI Visuals

	[SerializeField]
	private Animator animator;


	#endregion

	[Header("WarrirorChickenAI Sounds")]
	#region WarrirorChickenAI Sounds

	[SerializeField]
	private EventReference walkingSoundReference;

	[SerializeField]
	private EventReference runningSoundReference;

	[SerializeField]
	private EventReference jumpSoundReference;

	[SerializeField]
	private EventReference attackSoundReference;

	[NonSerialized]
	private EventInstance walkingSoundInstance;

	[NonSerialized]
	private EventInstance runningSoundInstance;


	#endregion

	#region WarrirorFoxAI Other

	[field: NonSerialized]
	public bool IsForcedToGoHome { get; private set; }


	#endregion


	// Initialize
	private void Start()
	{
		walkingSoundInstance = RuntimeManager.CreateInstance(walkingSoundReference);
		runningSoundInstance = RuntimeManager.CreateInstance(runningSoundReference);

		RuntimeManager.AttachInstanceToGameObject(walkingSoundInstance, this.transform);
		RuntimeManager.AttachInstanceToGameObject(runningSoundInstance, this.transform);
	}


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
		RuntimeManager.AttachInstanceToGameObject(walkingSoundInstance, this.transform);
		walkingSoundInstance.start();
		animator.Play("Walking");
		base.OnStateChangedToWalking();
	}

	protected override void OnStateChangedToRunning()
	{
		RuntimeManager.AttachInstanceToGameObject(runningSoundInstance, this.transform);
		runningSoundInstance.start();
		animator.Play("Running");
		base.OnStateChangedToRunning();
	}

	protected override void OnStateChangedToJumping()
	{
		RuntimeManager.PlayOneShot(jumpSoundReference, this.transform.position);
		animator.Play("Jumping");
		base.OnStateChangedToJumping();
	}

	protected override void OnStateChangedToAttacking()
	{
		RuntimeManager.PlayOneShot(attackSoundReference, this.transform.position);
		animator.Play("Attacking");
		base.OnStateChangedToAttacking();
	}

	protected override void OnStateChangedToDead()
	{
		PlayerControllerSingleton.onTargetDeathEventDict[TargetType.WarrirorChicken]?.Invoke();
		base.OnStateChangedToDead();
	}

	protected override void OnStateChangedToAny(PlayerStateType newState)
	{
		walkingSoundInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
		runningSoundInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);

		base.OnStateChangedToAny(newState);
	}

	public override void OnEnteredAIHome(HomeBase home)
	{
		IsForcedToGoHome = false;
		base.OnEnteredAIHome(home);
	}

	// Dispose
	protected override void OnDisable()
	{
		walkingSoundInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
		runningSoundInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);

		DoFrameDependentPhysics();
		IsForcedToGoHome = false;
		base.OnDisable();
	}

	private void OnDestroy()
	{
		walkingSoundInstance.release();
		runningSoundInstance.release();
	}
}


#if UNITY_EDITOR

public partial class WarrirorChickenAI
{ }

#endif