using FMOD.Studio;
using FMODUnity;
using System;
using UnityEngine;

public sealed partial class WarrirorFoxAI : GroundedWarrirorAIBase
{
	[Header("BabyChickenAI Visuals")]
	#region BabyChickenAI Visuals

	[SerializeField]
	private Animator animator;


	#endregion

	[Header("WarrirorFoxAI Sounds")]
	#region WarrirorFoxAI Sounds

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
	public bool IsCaughtMeal { get; private set; }


	#endregion


	// Initialize
	private void Start()
	{
		walkingSoundInstance = RuntimeManager.CreateInstance(walkingSoundReference);
		runningSoundInstance = RuntimeManager.CreateInstance(runningSoundReference);

		RuntimeManager.AttachInstanceToGameObject(walkingSoundInstance, this.transform);
		RuntimeManager.AttachInstanceToGameObject(runningSoundInstance, this.transform);

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
		PlayerControllerSingleton.onTargetDeathEventDict[TargetType.WarrirorFox]?.Invoke();
		base.OnStateChangedToDead();
	}

	protected override void OnStateChangedToAny(PlayerStateType newState)
	{
		walkingSoundInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
		runningSoundInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);

		base.OnStateChangedToAny(newState);
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
		walkingSoundInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
		runningSoundInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);

		IsCaughtMeal = false;
		base.OnDisable();
	}

	private void OnDestroy()
	{
		walkingSoundInstance.release();
		runningSoundInstance.release();

		DayCycleControllerSingleton.Instance?.onDaylightTypeChanged.RemoveListener(OnDaylightTypeChanged);
	}
}


#if UNITY_EDITOR

public sealed partial class WarrirorFoxAI
{ }

#endif