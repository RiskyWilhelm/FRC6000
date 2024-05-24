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

	[Header("Target")]
	public string[] allowedTargetTags = new string[0];

	[field: SerializeField]
	public virtual ushort Power { get; protected set; }

	[field: SerializeField]
	public virtual ushort Health { get; protected set; }


	// Initialize
	private void Start()
	{
		currentDestination = selfRigidbody.position;
	}


	// Update
	protected virtual void Update()
	{
		DetectObstaclesOnMovingDirection();
		DoState();
	}

	protected virtual void LateUpdate()
	{
		lastPosition = selfRigidbody.position;
	}

	protected void DetectObstaclesOnMovingDirection()
	{
		// If not able to go into next position, set position to last position. Useful for where player cant fall to bottom from an edge and obstacle detection
		if (!IsAbleToGo(selfRigidbody.position + (selfRigidbody.velocity * Time.deltaTime)))
		{
			ClearDestination();
			selfRigidbody.position = lastPosition;
		}
	}

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

			case AIState.Attacking:
			DoAttacking();
			break;

			case AIState.Dead:
			DoDead();
			break;
		}
	}

	protected void UpdateState()
	{
		// Wait for finishing the attack
		if (State == AIState.Attacking)
			return;
		else if (Health == 0)
			State = AIState.Dead;
		else if (selfRigidbody.position.x != currentDestination.x)
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

	protected virtual void DoAttacking()
	{ }

	protected virtual void DoDead()
	{ }

	public void SetDestinationTo(Vector2 newDestination)
	{
		currentDestination = newDestination;
	}

	public void ClearDestination()
	{
		currentDestination = selfRigidbody.position;
	}

	public bool TryGetTargetFromCollider<TargetType>(Collider2D collider, out TargetType foundTarget)
		where TargetType : IAITarget
	{
		// Check if event wants to reflect the collision. If there is no EventReflector, it is the main object that wants the event
		if (!EventReflector.TryGetReflectedGameObject(collider.gameObject, out GameObject colliderGameObject))
			colliderGameObject = collider.gameObject;

		// Try Get AI target
		foreach (var iteratedTag in allowedTargetTags)
			if (colliderGameObject.CompareTag(iteratedTag))
			{
				if (colliderGameObject.TryGetComponent<TargetType>(out foundTarget))
					return true;

				break;
			}

		foundTarget = default;
		return false;
	}

	public virtual void OnGotAttackedBy(AIBase chaser)
	{ }

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

	public virtual bool IsAbleToAttackTo(IAITarget target)
	{
		if ((target != null) && this.IsPowerfulThan(target))
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