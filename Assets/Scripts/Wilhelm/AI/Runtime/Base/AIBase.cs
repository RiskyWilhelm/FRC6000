using AYellowpaper;
using UnityEngine;

/// <remarks> A collider setup with <see cref="EventBase{EventType}"/> is should be done for <see cref="OnCaughtSomething(UnityEngine.Collider2D)"/>.
/// You can use <see cref="EventReflector"/> in order to reflect the event to base GameObject
/// </remarks>
[RequireComponent(typeof(Rigidbody2D))]
[DisallowMultipleComponent]
public abstract partial class AIBase : MonoBehaviour, IAITarget
{
	[Header("Movement")]
	[SerializeField]
	private Rigidbody2D selfRigidbody;

	public float horizontalVelocity = 5f;

	public float idleMovementRadius = 5f;

	[Tooltip("Used for casting to check if idle can walk through. Ideally, you should set this nearly but not same as the player size (radius)")]
	public float obstacleDetectorRadius = 1f;

	[SerializeField]
	private Timer idleMovementTimer = new(5f);

	private float overrideHorizontalVelocity;

	public virtual Vector2 Position => selfRigidbody.position;

	public AIState State { get; protected set; }

	[Header("Target")]
	public InterfaceReference<IAITarget> currentTarget;

	public string[] allowedTargetTags = new string[0];

	public bool IsHaveTarget => (currentTarget.UnderlyingValue != null);

	[field: SerializeField]
	public virtual byte Power { get; protected set; }

	[field: SerializeField]
	public float OthersMaxApproachDistance { get; protected set; }


	// Update
	protected virtual void Update()
	{
		DoState();
	}

	protected virtual void FixedUpdate()
	{
		MoveByVelocity();
	}

	protected void MoveByVelocity()
	{
		selfRigidbody.velocityX = overrideHorizontalVelocity;
	}

	public void OnCaughtSomething(Collider2D collider)
	{
		// Check if event wants to reflect the collision. If there is no EventReflector, it is the main object that wants the event
		if (!EventReflector.TryGetReflectedGameObject(collider.gameObject, out GameObject colliderGameObject))
			colliderGameObject = collider.gameObject;

		// Try to catch
        if (IsCatchable(colliderGameObject) && colliderGameObject.TryGetComponent<IAITarget>(out IAITarget foundTarget))
		{
			ClearTarget();
			foundTarget.OnGotCaughtBy(this);
		}
	}

	public bool IsCatchable(GameObject gameObject)
	{
		// Check if this(self) can catch the GameObject by tag (AITargetDummy tag is passing always)
		if (gameObject.CompareTag(Tags.AITargetDummy))
		{
			if (gameObject.GetComponent<AITargetDummy>().ownerAI == this)
				return true;
			else
				return false;
		}

		foreach (var iteratedTag in allowedTargetTags)
		{
			if (gameObject.CompareTag(iteratedTag))
				return true;
		}

		return false;
	}

	public virtual void OnGotCaughtBy(AIBase chaser)
	{ }

	protected virtual void UpdateState()
	{
		if (IsHaveTarget)
		{
			if (currentTarget.Value.IsChaseableBy(this))
				State = AIState.Chasing;
			else
				State = AIState.RunningAway;
		}
		else
			State = AIState.Idle;
	}

	protected virtual void DoState()
	{
		UpdateState();

		switch (State)
		{
			case AIState.Idle:
				if (idleMovementTimer.Tick() && (currentTarget.UnderlyingValue == null))
					DoIdle();
			break;

			case AIState.RunningAway:
				goto case AIState.Idle;

			case AIState.Chasing:
				DoChasing();
			break;
		}
	}

	protected virtual void DoIdle()
	{
		// Initialize random position
		var horizontalRandomPosition = Random.Range(-idleMovementRadius, idleMovementRadius);
		var newIdlePosition = selfRigidbody.position;
		newIdlePosition.x += horizontalRandomPosition;

		// If cant go to the newIdlePosition, try opposite direction
		if (!IsAbleToGo(newIdlePosition))
		{
			newIdlePosition.x -= horizontalRandomPosition;
			newIdlePosition.x += -horizontalRandomPosition;

			// If cant go to the opposite direction too, give up
			if (!IsAbleToGo(newIdlePosition))
				return;
		}

		// Initialize dummy
		var dummy = AITargetDummyPool.Get(newIdlePosition);
		dummy.ownerAI = this;

		currentTarget.UnderlyingValue = dummy;
	}

	protected void DoChasing()
	{
		if (!IsHaveTarget)
			return;

		// Chase until reaching the max approach distance of current target
		var distanceToTarget = (currentTarget.Value.Position - selfRigidbody.position);

		if (distanceToTarget.x >= currentTarget.Value.OthersMaxApproachDistance)
			overrideHorizontalVelocity = horizontalVelocity;
		else if (distanceToTarget.x <= -currentTarget.Value.OthersMaxApproachDistance)
			overrideHorizontalVelocity = -horizontalVelocity;
		else
			overrideHorizontalVelocity = 0;
	}

	public void ClearTarget()
	{
		overrideHorizontalVelocity = 0;
		currentTarget.UnderlyingValue = null;
	}

	/// <summary> Checks whether it can go to that position without any obstacles or falling places </summary>
	public bool IsAbleToGo(Vector2 worldPosition2D, int layerMask = Layers.Mask.Ground)
	{
		// Check if there is any obstacle in front of self
		var normObstacleDetectorDir = (worldPosition2D - selfRigidbody.position).normalized;
		var obstacleRaycast = Physics2D.CircleCast(selfRigidbody.position, obstacleDetectorRadius, normObstacleDetectorDir, Vector3.Distance(selfRigidbody.position, worldPosition2D), layerMask);

		if (obstacleRaycast.collider)
			return false;

		// TODO: Check if the place is not fallable. So the method is extendable.

		return true;
	}
}


#if UNITY_EDITOR

public abstract partial class AIBase
{
	private void OnDrawGizmosSelected()
	{
		// Display the explosion radius when selected
		Gizmos.color = new Color(1f, 0f, 0f, 0.25f);
		Gizmos.DrawSphere(this.transform.position, obstacleDetectorRadius);
	}
}

#endif