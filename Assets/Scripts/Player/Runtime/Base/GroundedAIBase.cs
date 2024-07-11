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
	private uint walkingForce;

	[SerializeField]
	[Tooltip("If you want single axis limited only, set other axis to zero")]
	private Vector2 walkingMaxVelocity;


	#endregion

	[Header("GroundedAIBase Running")]
	#region GroundedAIBase Running

	[SerializeField]
	private uint runningForce;

	[SerializeField]
	[Tooltip("If you want single axis limited only, set other axis to zero")]
	private Vector2 runningMaxVelocity;


	#endregion

	[Header("GroundedAIBase Jumping")]
	#region GroundedAIBase Jumping

	[SerializeField]
	[Tooltip("Decides when it should switch the state")]
	private Timer jumpingReleaseStateTimer = new(0.75f);

	[SerializeField]
	[Tooltip("Sticks the state")]
	private Timer jumpingBlockStateTimer = new(0.25f);

	[SerializeField]
	private uint jumpingForce;

	[SerializeField]
	[Range(0, 360)]
	[Tooltip("Decides what angle should be considered as jumpable")]
	private float jumpingAngle = 45f;


	#endregion

	#region GroundedAIBase Other

	[NonSerialized]
	protected sbyte norDirHorizontal;


	#endregion


	// Update
	protected override void FixedUpdate()
	{
		base.FixedUpdate();
		ResetPhysicsSyncGarbages();
	}

	protected void ResetPhysicsSyncGarbages()
	{
		norDirHorizontal = 0;
	}

	protected void LimitVelocity(Vector2 maxVelocity)
	{
		if (maxVelocity.x != 0)
			selfRigidbody.velocityX = Math.Clamp(selfRigidbody.velocityX, -maxVelocity.x, maxVelocity.x);

		if (maxVelocity.y != 0)
			selfRigidbody.velocityY = Math.Clamp(selfRigidbody.velocityY, -maxVelocity.y, maxVelocity.y);
	}

	[ContextMenu(nameof(Jump))]
	public void Jump()
	{
		if (State is PlayerStateType.Jumping)
			return;

		selfRigidbody.AddForceY(jumpingForce, ForceMode2D.Impulse);
		State = PlayerStateType.Jumping;
	}

	protected override void DoIdle()
	{
		// If not grounded, set state to Flying
		if (!IsGrounded())
		{
			idleTimer.Reset();
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

			if (this.TrySetDestinationToVector(newDestination, raycastBounds.x))
			{
				State = PlayerStateType.Walking;
				return;
			}
		}

		// Otherwise, continue old idle
		base.DoIdle();
	}

	protected override void DoWalking()
	{
		// If not grounded, set state to flying
		if (!IsGrounded())
		{
			State = PlayerStateType.Flying;
			return;
		}

		// If destination available, set state to walking
		if (TryGetDestination(out Vector2 worldDestination))
		{
			// If wants to jump, jump
			if (IsAbleToJumpTowardsDestination())
			{
				Jump();
				return;
			}

			// Set the horizontal direction that mirrors to FixedUpdate
			norDirHorizontal = (sbyte)Mathf.Sign((worldDestination - selfRigidbody.position).x);
			return;
		}

		// If there is no destination set, set state to idle
		State = PlayerStateType.Idle;
	}

	protected override void DoRunning()
	{
		// If not grounded, set state to flying
		if (!IsGrounded())
		{
			State = PlayerStateType.Flying;
			return;
		}

		// If destination available, set state to running
		if (TryGetDestination(out Vector2 worldDestination))
		{
			// If wants to jump, jump
			if (IsAbleToJumpTowardsDestination())
			{
				Jump();
				return;
			}

			// Set the horizontal direction that mirrors to FixedUpdate
			norDirHorizontal = (sbyte)Mathf.Sign((worldDestination - selfRigidbody.position).x);
			return;
		}

		// If there is no destination set, set state to idle
		State = PlayerStateType.Idle;
	}

	protected override void DoFlying()
	{
		if (IsGrounded())
		{
			State = PlayerStateType.Idle;
			return;
		}
	}

	protected override void DoJumping()
	{
		var isGrounded = IsGrounded();
		var isAbleToSwitchStates = jumpingReleaseStateTimer.Tick() || isGrounded;

		if (jumpingBlockStateTimer.Tick() && isAbleToSwitchStates)
		{
			jumpingBlockStateTimer.Reset();
			jumpingReleaseStateTimer.Reset();

			if (isGrounded)
				State = PlayerStateType.Idle;
			else
				State = PlayerStateType.Flying;
		}
	}

	protected override void DoWalkingFixed()
	{
		selfRigidbody.AddForceX(walkingForce * norDirHorizontal, ForceMode2D.Impulse);
		LimitVelocity(walkingMaxVelocity);
	}

	protected override void DoRunningFixed()
	{
		selfRigidbody.AddForceX(runningForce * norDirHorizontal, ForceMode2D.Impulse);
		LimitVelocity(runningMaxVelocity);
	}

	public bool IsAbleToJumpTowardsDestination()
	{
		// If destination set or self didnt reached to destination, check if direction is facing to given angle
		if (IsReachedToDestinationOrNotSet())
			return false;
		
		if (!TryGetDestination(out Vector2 worldDestination))
			return false;

		return IsAbleToJumpTowards(worldDestination);
	}

	public bool IsAbleToJumpTowards(Vector2 worldPosition)
	{
		var distSelfToDestination = (worldPosition - selfRigidbody.position);
		var isInsideAngle = Vector3.Angle(Vector2.up, distSelfToDestination) <= (jumpingAngle * 0.5f);
		return isInsideAngle;
	}

	public override void CopyTo(in AIBase main)
	{
		if (main is GroundedAIBase groundedAI)
		{
			// Idle
			groundedAI.idleMaxDistance = this.idleMaxDistance;
			groundedAI.idleTimer = this.idleTimer;

			// Walking
			groundedAI.walkingForce = this.walkingForce;
			groundedAI.walkingMaxVelocity = this.walkingMaxVelocity;

			// Running
			groundedAI.runningForce = this.runningForce;
			groundedAI.runningMaxVelocity = this.runningMaxVelocity;

			// Jump
			groundedAI.jumpingForce = this.jumpingForce;
			groundedAI.jumpingAngle = this.jumpingAngle;
			groundedAI.jumpingReleaseStateTimer = this.jumpingReleaseStateTimer;
		}

		base.CopyTo(main);
	}


	// Dispose
	protected override void OnDisable()
	{
		ResetPhysicsSyncGarbages();
		base.OnDisable();
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

		if (TryGetDestinationVector(out Vector2 worldDestination))
			distSelfToDestination = Vector2.Distance(worldDestination, selfRigidbody.position);

		// Draw
		Handles.color = new Color(0.5f, 0.5f, 0, 0.25f);
		Handles.DrawWireArc(selfRigidbody.position, Vector3.forward, Vector3.up.RotateByDegreeAngle(-jumpingAngle * 0.5f, Vector3.forward), jumpingAngle, Mathf.Clamp(distSelfToDestination, 1f, distSelfToDestination), Mathf.Clamp(distSelfToDestination, 1f, 5f));
	}
}

#endif


