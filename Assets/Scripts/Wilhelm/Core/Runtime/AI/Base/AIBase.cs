using UnityEngine;

/// <remarks> A collider setup with <see cref="EventBase{EventType}"/> is should be done for <see cref="OnNormalAttack(UnityEngine.Collider2D)"/>.
/// You can use <see cref="EventReflector"/> in order to reflect the event to base GameObject
/// </remarks>
[RequireComponent(typeof(Rigidbody2D))]
[DisallowMultipleComponent]
public abstract partial class AIBase : MonoBehaviour, IAITarget
{
	// Actions
	private AIState _state;

	public AIState State
	{
		get => _state;
		set
		{
			if (value != _state)
			{
				Debug.LogFormat("State of {0} is set to {1}", this.name, value);
				OnStateChanged(value);
			}

			_state = value;
		}
	}

	[Header("Movement")]
	[SerializeField]
	protected Rigidbody2D selfRigidbody;

	public float horizontalVelocity = 5f;

	private Vector2? currentDestination;

	private Transform currentDestinationTarget;

	/// <summary> Controls how many meters will be considered as reached when approaching the target </summary>
	private float destinationThresholdDistance;

	[Tooltip("Used for defining the player bounds. You should set this nearly but not same as the player size (radius)")]
	public float sizeRadius = 1f;

	[Header("Target")]
	[field: SerializeField]
	public virtual ushort Power { get; protected set; }

	[field: SerializeField]
	public virtual ushort Health { get; protected set; }


	// Update
	private void Update()
	{
		if (IsReachedToDestination())
		{
			OnReachedToDestination();
			ClearDestination();
		}
		
		DetectObstacles();
		DoState();
		OnUpdate();
	}

	protected virtual void OnUpdate()
	{ }

	private void DetectObstacles()
	{
		// If not able to go to next position and destination, clear destination and set position to last position. Useful for where player cant fall to bottom from an edge and obstacle detection
		var nextPredictedPosition = selfRigidbody.position + (selfRigidbody.velocity * Time.deltaTime);
		var hasDestination = TryGetDestination(out Vector2 destination);

		if (!IsAbleToGo(nextPredictedPosition) || (hasDestination && !IsAbleToGo(destination)))
			ClearDestination();
	}

	protected virtual void OnReachedToDestination()
	{ }

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

			case AIState.Attacking:
			DoAttacking();
			break;

			case AIState.Dead:
			DoDead();
			break;
		}
	}

	protected virtual void UpdateState()
	{
		// Wait for finishing the attack
		if (State == AIState.Attacking)
			return;
		else if (Health == 0)
			State = AIState.Dead;
		else if ((selfRigidbody.velocityX != 0) && !IsReachedToDestination())
			State = AIState.Running;
		else
			State = AIState.Idle;
	}

	protected virtual void OnStateChanged(AIState newState)
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

	protected virtual void DoIdle()
	{ }

	protected virtual void DoRunning()
	{
		// Run to the destination until the threshold exceeds
		if (TryGetDestination(out Vector2 destination))
		{
			float horizontalDistanceToDestination = (destination.x - selfRigidbody.position.x);

			if (horizontalDistanceToDestination > 0)
				selfRigidbody.velocityX = horizontalVelocity;
			else if (horizontalDistanceToDestination < 0)
				selfRigidbody.velocityX = -horizontalVelocity;
		}
	}

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

	public bool TryGetTargetFromCollider<TargetType>(Collider2D collider, out TargetType foundTarget)
		where TargetType : IAITarget
	{
		// Check if event wants to reflect the collision. If there is no EventReflector, it is the main object that wants the event
		if (!EventReflector.TryGetReflectedGameObject(collider.gameObject, out GameObject colliderGameObject))
			colliderGameObject = collider.gameObject;

		// Try Get AI target
		return colliderGameObject.TryGetComponent<TargetType>(out foundTarget);
	}

	public virtual void OnGotAttackedBy(AIBase chaser)
	{
		TakeDamage(chaser.Power);
	}

	public void TakeDamage(ushort damage)
	{
		if (Health != 0)
			this.Health -= (ushort)Mathf.Clamp(damage, ushort.MinValue, ushort.MaxValue);

		if (Health == 0)
			State = AIState.Dead;
	}

	/// <summary> Checks whether it can go to that position without any obstacles </summary>
	public bool IsAbleToGo(Vector2 worldPosition2D, int layerMask = Layers.Mask.Ground)
	{
		// Check if there is any obstacle in front of self
		var normObstacleDetectorDir = (worldPosition2D - selfRigidbody.position).normalized;
		var obstacleRaycast = Physics2D.CircleCast(selfRigidbody.position, sizeRadius, normObstacleDetectorDir, Vector3.Distance(selfRigidbody.position, worldPosition2D), layerMask);

		if (obstacleRaycast.collider)
			return false;

		// TODO: Check if the place is not fallable. So the method is extendable.

		return true;
	}

	public bool IsPowerfulThan(IAITarget otherAI) => Power >= otherAI.Power;
}


#if UNITY_EDITOR

public abstract partial class AIBase
{
	private void OnDrawGizmosSelected()
	{
		// Display the explosion radius when selected
		Gizmos.color = new Color(1f, 0f, 0f, 0.25f);
		Gizmos.DrawSphere(this.transform.position, sizeRadius);
	}
}

#endif