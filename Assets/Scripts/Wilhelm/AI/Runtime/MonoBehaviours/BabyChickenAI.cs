using System;
using UnityEngine;

public partial class BabyChickenAI : GroundedAIBase, IHomeAccesser
{
	#region BabyChickenAI Movement

	[SerializeField]
	private Timer goHomeBackTimer = new(10f, 10f, 60f);

	private bool mustGoHomeBack;


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
		mustGoHomeBack = false;
		goHomeBackTimer.Reset();
		ClearDestination();

		base.OnEnable();
	}


	// Update
	protected override void DoIdle()
	{
		bool isGoingHome = false;

		if (mustGoHomeBack)
			isGoingHome = TrySetDestinationToHome();

		if (!isGoingHome)
			base.DoIdle();
	}

	protected override void Update()
	{
		if ((DayCycleControllerSingleton.Instance.Time.daylightType is DaylightType.Night) || goHomeBackTimer.Tick())
			mustGoHomeBack = true;

		base.Update();
	}

	protected override void OnStateChangedToDead()
	{
		GameControllerSingleton.Instance.onChickenDeath?.Invoke();
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
		// If there is any AI that is powerful than self nearby, run!
		if (EventReflectorUtils.TryGetComponentByEventReflector<AIBase>(collider.gameObject, out AIBase foundTarget))
		{
			if (runawayTargetTypeList.Contains(foundTarget.TargetTag))
			{
				TrySetDestinationAwayFrom(foundTarget.transform.position);
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