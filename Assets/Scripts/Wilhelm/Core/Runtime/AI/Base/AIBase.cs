using System;
using UnityEngine;

/// <summary> Fresh base of AI. Implements destination system </summary>
[RequireComponent(typeof(Rigidbody2D))]
[DisallowMultipleComponent]
public abstract partial class AIBase : MonoBehaviour
{
	[Header("Movement")]
	[SerializeField]
	protected Rigidbody2D selfRigidbody;

	[field: SerializeField]
	[Tooltip("Used for defining the player bounds. You should set this nearly but not same as the player size (radius)")]
	public float SizeRadius { get; private set; } = 1f;

	private Vector2? currentDestination;

	private Transform currentDestinationTarget;

	/// <summary> Controls how many meters will be considered as reached when approaching the target </summary>
	private float destinationThresholdDistance;


	// Actions
	private AIState _state;

	public AIState State
	{
		get => _state;
		set
		{
			if (value != _state)
				OnStateChanged(value);

			_state = value;
		}
	}



	// Update
	private void Update()
	{
		if (IsReachedToDestination())
			ClearDestination();

		DetectObstacles();
		DoState();
	}

	private void DetectObstacles()
	{
		// If not able to go to next position and destination, clear destination and set position to last position. Useful for where player cant fall to bottom from an edge and obstacle detection
		var nextPredictedPosition = selfRigidbody.position + (selfRigidbody.velocity * Time.deltaTime);
		var hasDestination = TryGetDestination(out Vector2 destination);

		if (!IsAbleToGo(nextPredictedPosition) || (hasDestination && !IsAbleToGo(destination)))
			ClearDestination();
	}

	protected virtual void UpdateState()
	{
		// Freeze the state machine when these happens
		if (State is AIState.Attacking or AIState.Dead or AIState.Jumping)
			return;

		if (!IsReachedToDestination())
		{
			if ((selfRigidbody.velocity != Vector2.zero) && !IsGrounded())
				State = AIState.Flying;
			else if (selfRigidbody.velocityX != 0)
				State = AIState.Running;
		}
		else
			State = AIState.Idle;
	}

	private void DoState()
	{
		UpdateState();

		switch (State)
		{
			case AIState.Idle:
			DoIdle();
			break;

			case AIState.Running:
			DoRunning();
			break;

			case AIState.Flying:
			DoFlying();
			break;

			case AIState.Jumping:
			DoJumping();
			break;

			case AIState.Attacking:
			DoAttacking();
			break;

			case AIState.Dead:
			DoDead();
			break;
		}
	}

	protected virtual void OnStateChanged(AIState newState)
	{
		throw new NotImplementedException();
	}

	protected virtual void DoIdle()
	{ }

	protected virtual void DoRunning()
	{ }

	protected virtual void DoFlying()
	{ }

	protected virtual void DoJumping()
	{ }

	protected virtual void DoAttacking()
	{ }

	protected virtual void DoDead()
	{ }

	public bool IsReachedToDestination()
	{
		var hasDestination = TryGetDestination(out Vector2 destination);

		// If destinations dont have any value, consider as reached
		if (!hasDestination)
			return true;

		// If self position equals or inside the threshold of destination, consider as reached
		var distanceToDestination = (destination - selfRigidbody.position);
		if (distanceToDestination.sqrMagnitude <= (destinationThresholdDistance * destinationThresholdDistance))
			return true;

		return false;
	}

	public void SetDestinationTo(Vector2 newDestination, float destinationApproachThreshold = 0.5f)
	{
		currentDestination = newDestination;
		currentDestinationTarget = null;
		this.destinationThresholdDistance = destinationApproachThreshold;

		// Check if the new destination is already reached one, then clear it
		if (IsReachedToDestination())
			ClearDestination();
	}

	public void SetDestinationTo(Transform newDestination, float destinationApproachThreshold = 0.5f)
	{
		currentDestination = null;
		currentDestinationTarget = newDestination;
		this.destinationThresholdDistance = destinationApproachThreshold;

		// Check if the new destination is already reached one, then clear it
		if (IsReachedToDestination())
			ClearDestination();
	}

	/// <summary> The destination supports transforms too so it gets the one which had set </summary>
	public bool TryGetDestination(out Vector2 worldPosition)
	{
		if (currentDestination.HasValue)
		{
			worldPosition = currentDestination.Value;
			return true;
		}
		else if (currentDestinationTarget != null)
		{
			worldPosition = currentDestinationTarget.position;
			return true;
		}

		worldPosition = default;
		return false;
	}

	public void ClearDestination()
	{
		currentDestination = null;
		currentDestinationTarget = null;
		destinationThresholdDistance = 0;
	}

	public virtual bool IsGrounded()
	{
		var groundRaycast = Physics2D.BoxCast(selfRigidbody.position, new Vector2(SizeRadius, 1), 0, Vector2.down, SizeRadius, Layers.Mask.Ground);

		if (groundRaycast.collider)
			return true;

		return false;
	}

	/// <summary> Checks whether it can go to that position without any obstacles </summary>
	public abstract bool IsAbleToGo(Vector2 worldPosition2D, int layerMask = Layers.Mask.Ground);
}


public abstract partial class AIBase<StatsType> : AIBase
	where StatsType : AIStats
{
	[field: SerializeField]
	public StatsType Stats { get; private set; }


	// Update
	protected override void OnStateChanged(AIState newState)
	{
		switch (newState)
		{
			case AIState.Idle:
			selfRigidbody.velocityX = 0;
			break;

			case AIState.Dead:
			selfRigidbody.velocityX = 0;
			break;
		}
	}

	protected override void DoRunning()
	{
		// Run to the destination until the threshold exceeds
		if (TryGetDestination(out Vector2 destination))
		{
			float horizontalDistanceToDestination = (destination.x - selfRigidbody.position.x);

			// Sets the velocity depends on the horizontal direction
			selfRigidbody.velocityX = Stats.Velocity * Mathf.Sign(horizontalDistanceToDestination);
		}
	}

	public void TakeDamage(ushort damage)
	{
		Stats.DecreaseHealth(damage);

		if (Stats.IsDead)
			State = AIState.Dead;
	}

	public override bool IsAbleToGo(Vector2 worldPosition2D, int layerMask = 8)
	{
		// Check if there is any obstacle in front of self
		var normObstacleDetectorDir = (worldPosition2D - selfRigidbody.position).normalized;
		var obstacleRaycast = Physics2D.CircleCast(selfRigidbody.position, SizeRadius, normObstacleDetectorDir, Vector3.Distance(selfRigidbody.position, worldPosition2D), layerMask);

		if (obstacleRaycast.collider)
			return false;

		return true;
	}

	public bool IsPowerfulThan<TStatsType>(AIBase<TStatsType> otherAI)
		where TStatsType : AIStats
	{
		return	Stats.Power >= otherAI.Stats.Power;
	}
}


#if UNITY_EDITOR

public abstract partial class AIBase<StatsType>
{
	private void OnDrawGizmosSelected()
	{
		// Display the explosion radius when selected
		Gizmos.color = new Color(1f, 0f, 0f, 0.25f);
		Gizmos.DrawSphere(this.transform.position, SizeRadius);
	}
}

#endif