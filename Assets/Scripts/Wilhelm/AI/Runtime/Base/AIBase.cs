using UnityEngine;

/// <remarks> A collider setup with <see cref="EventBase{EventType}"/> is should be done for <see cref="OnNormalAttack(UnityEngine.Collider2D)"/>.
/// You can use <see cref="EventReflector"/> in order to reflect the event to base GameObject
/// </remarks>
[RequireComponent(typeof(Rigidbody2D))]
[DisallowMultipleComponent]
public abstract partial class AIBase : MonoBehaviour, IAITarget
{
	private AIState _state;

	public AIState State
	{
		get => _state;
		set
		{
			if (value != _state)
				StateChanged(value);

			_state = value;
		}
	}

	[Header("Movement")]
	[SerializeField]
	private Rigidbody2D selfRigidbody;

	public float horizontalVelocity = 5f;

	private Vector2 lastPosition;

	private Vector2 currentDestination;

	[Tooltip("Used for defining the player bounds. You should set this nearly but not same as the player size (radius)")]
	public float sizeRadius = 1f;

	public bool IsReachedDestination => (currentDestination != selfRigidbody.position);

	[Header("Target")]
	public string[] allowedTargetTags = new string[0];

	[field: SerializeField]
	public virtual byte Power { get; protected set; }

	// Initialize
	private void Start()
	{
		currentDestination = selfRigidbody.position;
	}


	// Update
	protected virtual void Update()
	{
		// If not able to go into next position, set position to last position. Useful for where player cant fall to bottom from an edge
		if (!IsAbleToGo(selfRigidbody.position + (selfRigidbody.velocity * Time.deltaTime)))
		{
			ClearDestination();
			selfRigidbody.position = lastPosition;
		}

		DoState();
		lastPosition = selfRigidbody.position;
	}

	// You must call that method by yourself
	public bool TryGetTargetFromCollider(Collider2D collider, out IAITarget foundTarget)
	{
		// Check if event wants to reflect the collision. If there is no EventReflector, it is the main object that wants the event
		if (!EventReflector.TryGetReflectedGameObject(collider.gameObject, out GameObject colliderGameObject))
			colliderGameObject = collider.gameObject;

		// Try to attack
		if (State != AIState.Attacking)
		{
			foreach (var iteratedTag in allowedTargetTags)
				if (gameObject.CompareTag(iteratedTag))
				{
					if (colliderGameObject.TryGetComponent<IAITarget>(out foundTarget))
						return true;

					break;
				}
		}

		foundTarget = null;
		return false;
	}

	public virtual void OnGotAttacked(AIBase chaser)
	{ }

	protected virtual void DoState()
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
		}
	}

	protected void UpdateState()
	{
		// Wait for finishing the attack
		if (State == AIState.Attacking)
			return;

		if (selfRigidbody.position.x != currentDestination.x)
			State = AIState.Running;
		else
			State = AIState.Idle;
	}

	protected virtual void StateChanged(AIState newState)
	{ }

	protected virtual void DoIdle()
	{
		selfRigidbody.velocityX = 0;
	}

	protected virtual void DoRunning()
	{
		if (!IsAbleToGo(currentDestination))
		{
			ClearDestination();
			return;
		}

		// Run to the destination until the threshold exceeds
		var horizontalDistanceToDestination = (currentDestination.x - selfRigidbody.position.x);

		if (horizontalDistanceToDestination >= 0.5f)
			selfRigidbody.velocityX = horizontalVelocity;
		else if (horizontalDistanceToDestination <= -0.5f)
			selfRigidbody.velocityX = -horizontalVelocity;
		else
			selfRigidbody.velocityX = 0;
	}

	public void SetDestination(Vector2 newDestination)
	{
		if (IsAbleToGo(newDestination))
			currentDestination = newDestination;
		else
			currentDestination = selfRigidbody.position;
	}

	public void ClearDestination()
	{
		currentDestination = selfRigidbody.position;
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

	public virtual bool IsAbleToAttackToTarget(IAITarget currentTarget)
	{
		if ((currentTarget != null) && this.IsPowerfulThan(currentTarget))
			return true;

		return false;
	}
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