using System;
using UnityEngine;

public abstract partial class GroundedAIBase : AIBase
{
	[Header("GroundedAIBase Movement")]
	#region GroundedAIBase Movement

	[SerializeField]
	protected float movementForce = 0.5f;

	[SerializeField]
	protected float maxVelocityX = 5f;

	[NonSerialized]
	private sbyte norDirRunningToX;

	#endregion

	[Header("GroundedAIBase Idle")]
	#region GroundedAIBase Idle

	[SerializeField, Range(0, 255)]
	private byte idleMaxDistance = 10;

	[SerializeField]
	private Timer idleTimer = new(2f);

	#endregion


	// Initialize
	protected override void OnEnable()
	{
		norDirRunningToX = 0;
		base.OnEnable();
	}


	// Update
	protected override void DoIdle()
	{
		if (idleTimer.Tick())
		{
			// TODO: This will get broken when there is a ladder
			// Do idle when the timer finishes
			// Find a random horizontal position around self and set destination
			var randomHorizontalPosition = UnityEngine.Random.Range(-idleMaxDistance, idleMaxDistance);
			var newDestination = selfRigidbody.position;
			newDestination.x += randomHorizontalPosition;

			SetDestinationTo(newDestination);
			base.DoIdle();
		}
	}

	protected override void DoRunning()
	{
		// TODO: If jump will be added, this one must be refactored
		// If there is a destination, get the direction to go by using Mathf.Sign which gives 1 or -1 based on positivity of value
		if (TryGetDestination(out Vector2 worldDestination))
		{
			norDirRunningToX = (sbyte)Mathf.Sign((worldDestination - selfRigidbody.position).x);
			base.DoRunning();
		}
	}

	protected virtual void FixedUpdate()
	{
		// Do running
		if (norDirRunningToX != 0)
		{
			selfRigidbody.AddForceX(movementForce * norDirRunningToX, ForceMode2D.Impulse);
			norDirRunningToX = 0;
		}

		// Clamp the speed
		selfRigidbody.velocityX = Math.Clamp(selfRigidbody.velocityX, -maxVelocityX, maxVelocityX);
	}

	public override void CopyTo(in AIBase main)
	{
		if (main is GroundedAIBase groundedAI)
		{
			groundedAI.movementForce = this.movementForce;
			groundedAI.maxVelocityX = this.maxVelocityX;
			groundedAI.idleMaxDistance = this.idleMaxDistance;
			groundedAI.idleTimer = this.idleTimer;
		}

		base.CopyTo(main);
	}
}


#if UNITY_EDITOR

public abstract partial class GroundedAIBase
{ }

#endif