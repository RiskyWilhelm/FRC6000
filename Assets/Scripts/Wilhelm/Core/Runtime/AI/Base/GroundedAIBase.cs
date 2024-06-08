using System;
using UnityEngine;

public abstract partial class GroundedAIBase : AIBase
{
	[Header("GroundedAIBase Movement")]
	#region

	[SerializeField]
	protected float movementForce = 10;

	[SerializeField]
	protected float maxVelocityX = 10;

	[NonSerialized]
	private sbyte norDirRunningToX;

	#endregion


	// Initialize
	protected override void OnEnable()
	{
		norDirRunningToX = 0;
		base.OnEnable();
	}


	// Update
	protected override void DoRunning()
	{
		// TODO: If jump will be added, this one must be refactored
		// If there is a destination, get the direction to go by using Mathf.Sign which gives 1 or -1 based on positivity of value
		if (TryGetDestination(out Vector2 worldDestination))
			norDirRunningToX = (sbyte)Mathf.Sign((worldDestination - selfRigidbody.position).normalized.x);

		base.DoRunning();
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
		}

		base.CopyTo(main);
	}
}


#if UNITY_EDITOR

public abstract partial class GroundedAIBase
{ }

#endif