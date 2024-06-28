using System;
using System.Collections.Generic;
using UnityEngine;

public sealed partial class BabyChickenAI : GroundedAIBase, IHomeAccesser
{
	[Header("BabyChickenAI Movement")]
	#region BabyChickenAI Movement

	[SerializeField]
	private TimerRandomized goHomeBackTimer = new(10f, 10f, 60f);


	#endregion

	#region BabyChickenAI Other

	[field: NonSerialized]
	public bool OpenAIHomeGate { get; private set; }

	[field: NonSerialized]
	public HomeBase ParentHome { get; set; }


	#endregion
	

	// Initialize
	protected override void OnEnable()
	{
		GameControllerSingleton.Instance.onTargetBirthDict[TargetType.BabyChicken]?.Invoke();
		goHomeBackTimer.ResetAndRandomize();
		ClearDestination();

		base.OnEnable();
	}


	// Update
	protected override void Update()
	{
		goHomeBackTimer.Tick();
		base.Update();
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
		GameControllerSingleton.Instance.onTargetDeathDict[TargetType.BabyChicken]?.Invoke();
		ReleaseOrDestroySelf();
		base.OnStateChangedToDead();
	}

	public bool TrySetDestinationToHome()
	{
		if (ParentHome != null)
			return OpenAIHomeGate = TrySetDestinationTo(ParentHome.transform, 0.1f);

		return false;
	}

	protected override void OnChangedDestination(Vector2? newDestination)
	{
		OpenAIHomeGate = false;
		base.OnChangedDestination(newDestination);
	}

	public void OnRunawayTriggerStay2D(Collider2D collider)
	{
		if (EventReflectorUtils.TryGetComponentByEventReflector<AIBase>(collider.gameObject, out AIBase foundTarget))
			TryRunawayFrom(foundTarget);
	}

	private bool TryRunawayFrom(AIBase otherAI)
	{
		if (!runawayTargetTypeList.Contains(otherAI.TargetTag))
			return false;

		if (TrySetDestinationAwayFrom(otherAI.transform.position))
		{
			State = PlayerStateType.Running;
			OpenAIHomeGate = true;
			return true;
		}

		return false;
	}

	public void OnEnteredAIHome(HomeBase home)
	{
		ReleaseOrDestroySelf();
	}

	public void OnLeftFromAIHome(HomeBase home)
	{
		OpenAIHomeGate = false;
	}

	public void OnStolenByFoxHome(FoxAIHome foxAIHome)
	{
		GameControllerSingleton.Instance.onTargetDeathDict[TargetType.BabyChicken]?.Invoke();
		ReleaseOrDestroySelf();
	}

	public override void CopyTo(in AIBase main)
	{
		if (main is BabyChickenAI foundSelf)
			foundSelf.goHomeBackTimer = this.goHomeBackTimer;

		base.CopyTo(main);
	}
}


#if UNITY_EDITOR

public sealed partial class BabyChickenAI
{ }

#endif