using System;
using UnityEngine;

public partial class BabyChickenAI : GroundedAIBase, IAIHomeAccesser
{
	#region BabyChickenAI Other

	[field: SerializeField]
	public bool OpenAIHomeGate { get; protected set; }

	[field: NonSerialized]
	public AIHomeBase ParentHome { get; set; }

	#endregion


	// Initialize
	protected override void OnEnable()
	{
		ClearDestination();
		base.OnEnable();
	}


	// Update
	protected override void DoIdle()
	{
		// If the time is night-time, try go to the base. If there is no base, do the idle
		switch (DayCycleControllerSingleton.Instance.Time.daylightType)
		{
			case DaylightType.Light:
				goto default;

			case DaylightType.Night:
			{
				if (!TrySetDestinationToHome())
					goto default;
			}
			break;

			default:
				base.DoIdle();
			break;
		}
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

	protected override void OnStateChangedToDead()
	{
		ReleaseOrDestroySelf();
		base.OnStateChangedToDead();
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

	public void OnEnteredAIHome(AIHomeBase home)
	{
		ReleaseOrDestroySelf();
	}

	public void OnLeftFromAIHome(AIHomeBase home)
	{
		OpenAIHomeGate = false;
	}
}


#if UNITY_EDITOR

public partial class BabyChickenAI
{ }

#endif