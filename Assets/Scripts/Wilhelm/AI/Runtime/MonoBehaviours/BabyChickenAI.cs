using System;
using UnityEngine;

public sealed partial class BabyChickenAI : GroundedAIBase, IAIHomeAccesser
{
	[Header("BabyChickenAI Movement")]
	#region

	[SerializeField, Range(0, 255)]
	private byte idleMaxDistance = 10;

	[SerializeField]
	private Timer idleTimer = new (2f);

	#endregion

	#region Other

	[field: NonSerialized]
	public bool OpenAIHomeGate { get; private set; }

	#endregion


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
				var isFoundNearbyChickenBase = TrySetDestinationToNearestChickenBase();

				if (!isFoundNearbyChickenBase)
					goto default;
			}
			break;

			default:
			{
				if (idleTimer.Tick())
				{
					// Do idle when the timer finishes
					// Find a random horizontal position around self and set destination
					var randomHorizontalPosition = UnityEngine.Random.Range(-idleMaxDistance, idleMaxDistance);
					var newDestination = selfRigidbody.position;
					newDestination.x += randomHorizontalPosition;

					SetDestinationTo(newDestination);
				}
			}
			break;
		}


		base.DoIdle();
	}

	public bool TrySetDestinationToNearestChickenBase()
	{
		if (TagObject.TryGetNearestTagObject(this.transform, Tags.ChickenAIHome, out Transform nearestChickenBase,
			(iteratedTagObject) => IsAbleToGoTo(iteratedTagObject.position)))
		{
			OpenAIHomeGate = true;
			SetDestinationTo(nearestChickenBase, 0.1f);
			return true;
		}

		return false;
	}

	protected override void OnStateChangedToDead()
	{
		ReleaseOrDestroySelf();
		base.OnStateChangedToDead();
	}

	public void OnRunawayTriggerStay2D(Collider2D collider)
	{
		// If there is any AI that is powerful than self nearby, run!
		if (EventReflector.TryGetComponentByEventReflector<AIBase>(collider.gameObject, out AIBase foundTarget)
			&& foundTarget.IsPowerfulThan(this))
		{
			SetDestinationToAwayFrom(foundTarget.transform.position);
		}
	}

	public override void CopyTo(in AIBase main)
	{
		if (main is BabyChickenAI babyChickenAI)
		{
			babyChickenAI.idleMaxDistance = this.idleMaxDistance;
			babyChickenAI.idleTimer = this.idleTimer;
		}

		base.CopyTo(main);
	}

	public void OnEnteredAIHome(AIHomeBase home)
	{
		OpenAIHomeGate = false;
		ReleaseOrDestroySelf();
	}

	public void OnLeftFromAIHome(AIHomeBase home)
	{ }
}


#if UNITY_EDITOR

public sealed partial class BabyChickenAI
{ }

#endif