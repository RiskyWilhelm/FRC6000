using System;
using UnityEngine;

public partial class BabyChickenAI : GroundedAIBase, IHomeAccesser
{
	[Header("BabyChickenAI Movement")]
	#region BabyChickenAI Movement

	[SerializeField]
	private TimerRandomized goHomeBackTimer = new(10f, 10f, 60f);


	#endregion

	#region BabyChickenAI Other

	[field: NonSerialized]
	public bool OpenAIHomeGate { get; protected set; }

	[field: NonSerialized]
	public HomeBase ParentHome { get; set; }

	#endregion
	

	// Initialize
	protected override void OnEnable()
	{
		goHomeBackTimer.ResetAndRandomize();
		ClearDestination();

		base.OnEnable();
	}


	// Update
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

	protected override void Update()
	{
		if (DayCycleControllerSingleton.Instance.Time.daylightType is DaylightType.Night)
			goHomeBackTimer.Tick();

		base.Update();
	}

	protected override void OnStateChangedToDead()
	{
		GameControllerSingleton.Instance.onTargetDeathDict[TargetType.BabyChicken]?.Invoke();
		ReleaseOrDestroySelf();
		base.OnStateChangedToDead();
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

	protected override void OnChangedDestination(Vector2? newDestination)
	{
		OpenAIHomeGate = false;
		base.OnChangedDestination(newDestination);
	}

	public void OnRunawayTriggerStay2D(Collider2D collider)
	{
		if (EventReflectorUtils.TryGetComponentByEventReflector<AIBase>(collider.gameObject, out AIBase foundTarget))
		{
			if (!runawayTargetTypeList.Contains(foundTarget.TargetTag))
				return;
				
			if (TrySetDestinationAwayFrom(foundTarget.transform))
			{
				State = PlayerStateType.Running;
				OpenAIHomeGate = true;
			}
		}
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

public partial class BabyChickenAI
{ }

#endif