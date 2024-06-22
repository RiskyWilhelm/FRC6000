using System;
using UnityEditor;
using UnityEngine;

public abstract partial class GroundedAIBase : AIBase
{
	[Header("GroundedAIBase Idle")]
	#region GroundedAIBase Idle

	[SerializeField, Range(0, 255)]
	private byte idleMaxDistance = 10;

	[SerializeField]
	private Timer idleTimer = new(2f);

	#endregion

	[Header("GroundedAIBase Walking")]
	#region GroundedAIBase Walking

	[SerializeField]
	private uint walkForce;

	[SerializeField]
	[Tooltip("If you want single axis limited only, set other axis to zero")]
	private Vector2 walkMaxVelocity;

	#endregion

	[Header("GroundedAIBase Running")]
	#region GroundedAIBase Running

	[SerializeField]
	private uint runningForce;

	[SerializeField]
	[Tooltip("If you want single axis limited only, set other axis to zero")]
	private Vector2 runningMaxVelocity;

	#endregion

	[Header("GroundedAIBase Jump")]
	#region GroundedAIBase Jump

	[SerializeField]
	private Timer jumpReleaseStateTimer = new(0.5f);

	[SerializeField]
	private uint jumpForce;

	[SerializeField]
	[Range(0, 360)]
	[Tooltip("Decides what angle should be considered as jumpable")]
	protected float jumpAngle = 45f;

	#endregion

	#region GroundedAIBase Other

	[NonSerialized]
	private sbyte norDirHorizontal;

	#endregion


	// Initialize
	protected override void OnEnable()
	{
		ResetStateGarbages();
		base.OnEnable();
	}


	// Update
	protected virtual void FixedUpdate()
	{
		// Use state garbages to use states
		switch (State)
		{
			case PlayerStateType.Walking:
			{
				DoWalkingFixedUpdate();
				LimitVelocity(walkMaxVelocity);
			}
			break;

			case PlayerStateType.Running:
			{
				DoRunningFixedUpdate();
				LimitVelocity(runningMaxVelocity);
			}
			break;
		}

		ResetStateGarbages();
	}

	// TODO: This will get broken when there is a ladder
	protected override void DoIdle()
	{
		// If not grounded, set state to Flying
		if (!IsGrounded())
		{
			State = PlayerStateType.Flying;
			return;
		}

		// Find a random horizontal position around self and try set destination, if succeeded, set state to Walking
		if (idleTimer.Tick())
		{
			idleTimer.Reset();

			var randomHorizontalPosition = UnityEngine.Random.Range(-idleMaxDistance, idleMaxDistance);
			var newDestination = selfRigidbody.position;
			newDestination.x += randomHorizontalPosition;

			if (TrySetDestinationTo(newDestination, raycastBounds.x))
				State = PlayerStateType.Walking;
			else
				base.DoIdle();
		}
	}

	protected override void DoWalking()
	{
		// If cant get destination, set state to Idle
		if (!TryGetDestination(out Vector2 worldDestination))
		{
			if (IsGrounded())
				State = PlayerStateType.Idle;
			else
				State = PlayerStateType.Flying;

			return;
		}

		// If wants to jump, jump
		if (IsAbleToJumpToDestination())
		{
			Jump();
			return;
		}

		// Set the horizontal direction that mirrors to FixedUpdate
		norDirHorizontal = (sbyte)Mathf.Sign((worldDestination - selfRigidbody.position).x);
		base.DoWalking();
	}

	protected virtual void DoWalkingFixedUpdate()
	{
		selfRigidbody.AddForceX(walkForce * norDirHorizontal, ForceMode2D.Impulse);
	}

	protected override void DoRunning()
	{
		// If cant get destination, set state to Idle
		if (!TryGetDestination(out Vector2 worldDestination))
		{
			if (IsGrounded())
				State = PlayerStateType.Idle;
			else
				State = PlayerStateType.Flying;

			return;
		}

		// If wants to jump, jump
		if (IsAbleToJumpToDestination())
		{
			Jump();
			return;
		}

		// Set the horizontal direction that mirrors to FixedUpdate
		norDirHorizontal = (sbyte)Mathf.Sign((worldDestination - selfRigidbody.position).x);
		base.DoRunning();
	}

	protected virtual void DoRunningFixedUpdate()
	{
		selfRigidbody.AddForceX(runningForce * norDirHorizontal, ForceMode2D.Impulse);
	}

	protected override void DoFlying()
	{
		if (IsGrounded())
		{
			State = PlayerStateType.Idle;
			return;
		}

		base.DoFlying();
	}

	protected override void DoJumping()
	{
		var isGrounded = IsGrounded();

		if (jumpReleaseStateTimer.Tick() || isGrounded)
		{
			jumpReleaseStateTimer.Reset();

			if (isGrounded)
				State = PlayerStateType.Idle;
			else
				State = PlayerStateType.Flying;
		}

		base.DoJumping();
	}

	public void Jump()
	{
		selfRigidbody.AddForceY(jumpForce, ForceMode2D.Impulse);
		State = PlayerStateType.Jumping;
	}

	/// <summary> Resets values that used to create synchronization with FixedUpdate Physics </summary>
	protected void ResetStateGarbages()
	{
		norDirHorizontal = 0;
	}

	/// <param name="maxVelocity"> If you want single axis limited only, set other axis to zero </param>
	protected void LimitVelocity(Vector2 maxVelocity)
	{
		if (maxVelocity.x != 0)
			selfRigidbody.velocityX = Math.Clamp(selfRigidbody.velocityX, -maxVelocity.x, maxVelocity.x);

		if (maxVelocity.y != 0)
			selfRigidbody.velocityY = Math.Clamp(selfRigidbody.velocityY, -maxVelocity.y, maxVelocity.y);
	}

	public bool IsAbleToJumpToDestination()
	{
		// If destination set or self didnt reached to destination, check if direction is facing to given angle
		if (IsReachedToDestination())
			return false;
		
		if (!TryGetDestination(out Vector2 worldDestination))
			return false;

		return IsAbleToJumpTo(worldDestination);
	}

	public bool IsAbleToJumpTo(Vector2 worldPosition)
	{
		// Prepare values
		var distSelfToDestination = (worldPosition - selfRigidbody.position);

		// Prepare check values
		var isInsideAngle = Vector3.Angle(Vector2.up, distSelfToDestination) <= (jumpAngle * 0.5f);
		var isNotTallerThanPlayer = distSelfToDestination.sqrMagnitude <= (raycastBounds.y * raycastBounds.y);

		return isInsideAngle && isNotTallerThanPlayer;
	}

	public override void CopyTo(in AIBase main)
	{
		if (main is GroundedAIBase groundedAI)
		{
			// Idle
			groundedAI.idleMaxDistance = this.idleMaxDistance;
			groundedAI.idleTimer = this.idleTimer;

			// Walking
			groundedAI.walkForce = this.walkForce;
			groundedAI.walkMaxVelocity = this.walkMaxVelocity;

			// Jump
			groundedAI.jumpForce = this.jumpForce;
			groundedAI.jumpAngle = this.jumpAngle;
			groundedAI.jumpReleaseStateTimer = this.jumpReleaseStateTimer;
		}

		base.CopyTo(main);
	}
}


#if UNITY_EDITOR

public abstract partial class GroundedAIBase
{
	protected override void OnDrawGizmosSelected()
	{
		DrawJumpGizmos();
		base.OnDrawGizmosSelected();
	}

	private void DrawJumpGizmos()
	{
		// Prepare angles
		var distSelfToDestination = raycastBounds.y * 1.5f;

		if (TryGetDestination(out Vector2 worldDestination))
			distSelfToDestination = Vector2.Distance(worldDestination, selfRigidbody.position);

		// Draw
		Handles.color = new Color(0.5f, 0.5f, 0, 0.25f);
		Handles.DrawWireArc(selfRigidbody.position, Vector3.forward, Vector3.up.Rotate(-jumpAngle * 0.5f, Vector3.forward), jumpAngle, Mathf.Clamp(distSelfToDestination, 1f, distSelfToDestination), Mathf.Clamp(distSelfToDestination, 1f, 5f));
	}
}

#endif


