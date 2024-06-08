using System;
using UnityEngine;
using UnityEngine.Pool;

/// <summary> Fresh base of AI. Implements destination system </summary>
[RequireComponent(typeof(Rigidbody2D))]
[DisallowMultipleComponent]
public abstract partial class AIBase : MonoBehaviour, IAITarget, IPooledObject<AIBase>, ICopyable<AIBase> 
{
	[Header("AIBase Movement")]
	#region

	[SerializeField]
	protected Rigidbody2D selfRigidbody;

	[NonSerialized]
	private Vector2? currentDestination;

	[NonSerialized]
	private Transform currentDestinationTarget;

	/// <summary> Controls how many meters will be considered as reached when approaching the target </summary>
	[NonSerialized]
	private float destinationThresholdDistance;

	[field: SerializeField]
	[field: Tooltip("Used for defining the bounds. You should set this nearly but not same as the collider size because this is used for raycasting")]
	public Vector2 Bounds { get; private set; } = new (1f, 1f);

	#endregion

	[field: Header("AIBase Stats")]
	#region Stats

	[field: SerializeField]
	public ushort Health { get; private set; }

	[field: SerializeField]
	public ushort Power { get; private set; }

	public bool IsDead => (Health == 0);

	#endregion

	#region Other

	private AIState _state;

	public AIState State
	{
		get => _state;
		set
		{
			if (value != _state)
			{
				_state = value;
				OnStateChanged(value);
			}
		}
	}

	public IPool<AIBase> ParentPool { get; set; }


	#endregion


	// Initialize
	protected virtual void OnEnable()
	{
		State = AIState.Idle;
	}


	// Update
	private void Update()
	{
		if (IsReachedToDestination())
		{
			TryGetDestination(out Vector2 reachedDestination);
			OnReachedToDestination(reachedDestination);
			ClearDestination();
		}

		DetectObstacles();
		UpdateState();
		DoState();
	}

	private void DetectObstacles()
	{
		// If not able to go to next position and destination, clear destination and set position to last position. Useful for where player cant fall to bottom from an edge and obstacle detection
		var nextPredictedPosition = selfRigidbody.position + (selfRigidbody.velocity * Time.deltaTime);
		var hasDestination = TryGetDestination(out Vector2 destination);

		if (!IsAbleToGoTo(nextPredictedPosition) || (hasDestination && !IsAbleToGoTo(destination)))
			ClearDestination();
	}

	private void UpdateState()
	{
		// Freeze the state machine when these happens
		if (State is AIState.Attacking or AIState.Dead or AIState.Jumping)
			return;

		if (!IsReachedToDestination())
		{
			if (!IsGrounded())
				State = AIState.Flying;
			else
				State = AIState.Running;
		}
		else
			State = AIState.Idle;
	}

	private void DoState()
	{
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

	private void OnStateChanged(AIState newState)
	{
		switch (newState)
		{
			case AIState.Idle:
			OnStateChangedToIdle();
			break;

			case AIState.Running:
			OnStateChangedToRunning();
			break;

			case AIState.Flying:
			OnStateChangedToFlying();
			break;

			case AIState.Jumping:
			OnStateChangedToJumping();
			break;

			case AIState.Attacking:
			OnStateChangedToAttacking();
			break;

			case AIState.Dead:
			OnStateChangedToDead();
			break;
		}
	}

	protected virtual void DoIdle()
	{ }

	protected virtual void OnStateChangedToIdle()
	{ }

	protected virtual void DoRunning()
	{ }

	protected virtual void OnStateChangedToRunning()
	{ }

	protected virtual void DoFlying()
	{ }

	protected virtual void OnStateChangedToFlying()
	{ }

	protected virtual void DoJumping()
	{ }

	protected virtual void OnStateChangedToJumping()
	{ }

	protected virtual void DoAttacking()
	{ }

	protected virtual void OnStateChangedToAttacking()
	{ }

	protected virtual void DoDead()
	{ }

	protected virtual void OnStateChangedToDead()
	{ }

	public virtual void TakeDamage(uint damage)
	{
		Health = (ushort)Math.Clamp(Health - (int)damage, ushort.MinValue, ushort.MaxValue);

		if (Health == ushort.MinValue)
			State = AIState.Dead;
	}

	protected virtual void OnChangedDestination(Vector2? newDestination)
	{ }

	protected virtual void OnReachedToDestination(Vector2 reachedDestination)
	{ }

	public void SetDestinationTo(Vector2 newDestination, float destinationApproachThreshold = 1f)
	{
		currentDestination = newDestination;
		currentDestinationTarget = null;
		this.destinationThresholdDistance = destinationApproachThreshold;

		// Check if the new destination is already reached one, then clear it
		if (IsReachedToDestination())
			ClearDestination();
		else
			OnChangedDestination(newDestination);
	}

	public void SetDestinationTo(Transform newDestination, float destinationApproachThreshold = 1f)
	{
		currentDestination = null;
		currentDestinationTarget = newDestination;
		this.destinationThresholdDistance = destinationApproachThreshold;

		// Check if the new destination is already reached one, then clear it
		if (IsReachedToDestination())
			ClearDestination();
		else
			OnChangedDestination(newDestination.position);
	}

	// OPTIMIZATION: Needed in future versions
	/// <param name="isGroundedOnly"> Get the grounded direction only </param>
	public void SetDestinationToAwayFrom(Vector2 target, float destinationApproachThreshold = 1f, float checkInDegree = -180f, float checkEveryDegree = (180 / 12), bool isGroundedOnly = false)
	{
		// Prevent from no check by keeping the values positive
		// Clamp checkInAngle and checkInEveryDegree to 360
		checkInDegree = MathF.Abs(checkInDegree) % 360f;
		checkEveryDegree = Math.Clamp(MathF.Abs(checkEveryDegree) % checkInDegree, 1f, 360f);

		// We will work with radians to prevent unnecessary conversion so convert the method parameters to radians
		var radCheckInDegree = checkInDegree * Mathf.Deg2Rad;
		var radCheckEveryDegree = checkEveryDegree * Mathf.Deg2Rad;

		// Get direction of 'target to self' and its angle in radians
		var norDirTargetToSelf = (selfRigidbody.position - target).normalized;
		var radTargetToSelfAngle = MathfUtil.Atan2_360(norDirTargetToSelf.y, norDirTargetToSelf.x);

		// Get starting and ending point in radians
		var radStartAngle = radTargetToSelfAngle - (radCheckInDegree * 0.5f);
		var radEndAngle = radTargetToSelfAngle + (radCheckInDegree * 0.5f);
		var radCurrentAngle = radStartAngle;

		// Ready the list for storing the look directions. ValueTuple<float, Vector2> stores <DotProduct, WorldPositionDestination>
		var lookDirPooledList = ListPool<ValueTuple<float, Vector2>>.Get();

		// Do cast for every radCheckForEveryDegree and store valid one(s) look direction to check which angle is close enough to the norDirTargetToSelf direction
		while (radCurrentAngle <= radEndAngle)
		{
			// nor stands for "normalized". Multiplying the Bounds with random value because there should be no conflict between Bounds'es
			var norCurrentAngle = new Vector2(MathF.Cos(radCurrentAngle), MathF.Sin(radCurrentAngle));
			var newDestination = selfRigidbody.position + (norCurrentAngle * (Bounds * 3));

			// If nothing is on the way , add to look directions based on grounded state on newDestination
			if (IsAbleToGoTo(newDestination))
			{
				var newLookDirTuple = new ValueTuple<float, Vector2>(Vector2.Dot(norDirTargetToSelf, norCurrentAngle), newDestination);

				if (isGroundedOnly)
				{
					if (IsGroundedAt(newDestination))
						lookDirPooledList.Add(newLookDirTuple);
				}
				else
					lookDirPooledList.Add(newLookDirTuple);
			}

			// DEBUG
			Debug.DrawLine(selfRigidbody.position, selfRigidbody.position + norCurrentAngle, new Color(0.75f, 0.75f, 0.75f, 0.5f));

			radCurrentAngle += radCheckEveryDegree;
		}

		// Get the dot product which is close enough to pointing the same way as norDirTargetToSelf direction
		if (lookDirPooledList.Count > 0)
		{
			var closestNorDirTuple = lookDirPooledList[0];

            for (int i = 1; i < lookDirPooledList.Count; i++)
            {
				// Checks if iterated lookDirection facing the same direction as norDirTargetToSelf more than current closestNorDirTuple
				if (lookDirPooledList[i].Item1 > closestNorDirTuple.Item1)
					closestNorDirTuple = lookDirPooledList[i];
            }

			// DEBUG
			Debug.DrawLine(selfRigidbody.position, closestNorDirTuple.Item2, Color.green);

			// Finally, set the new destination
			SetDestinationTo(closestNorDirTuple.Item2, destinationApproachThreshold);
        }

		// DEBUG
		/*Debug.DrawLine(selfRigidbody.position, selfRigidbody.position + new Vector2(MathF.Cos(radTargetToSelfAngle), MathF.Sin(radTargetToSelfAngle)));
		Debug.DrawLine(selfRigidbody.position, selfRigidbody.position + new Vector2(MathF.Cos(radStartAngle), MathF.Sin(radStartAngle)), Color.cyan);
		Debug.DrawLine(selfRigidbody.position, selfRigidbody.position + new Vector2(MathF.Cos(radEndAngle), MathF.Sin(radEndAngle)), Color.red);*/

		// Dispose the list
		ListPool<ValueTuple<float, Vector2>>.Release(lookDirPooledList);
	}

	/// <summary> The destination supports transforms too so it gets the one which had set </summary>
	public bool TryGetDestination(out Vector2 worldDestination)
	{
		if (currentDestination.HasValue)
		{
			worldDestination = currentDestination.Value;
			return true;
		}
		else if (currentDestinationTarget && currentDestinationTarget.gameObject.activeSelf)
		{
			worldDestination = currentDestinationTarget.position;
			return true;
		}

		worldDestination = default;
		return false;
	}

	public void ClearDestination()
	{
		currentDestination = null;
		currentDestinationTarget = null;
		destinationThresholdDistance = 0;
		OnChangedDestination(null);
	}

	public bool IsReachedToDestination()
	{
		var hasDestination = TryGetDestination(out Vector2 destination);

		// If destinations dont have any value, consider as reached
		if (!hasDestination)
			return true;

		// If self position equals or inside the threshold of destination, consider as reached
		var distSelfToDestination = (destination - selfRigidbody.position);
		return distSelfToDestination.sqrMagnitude <= (destinationThresholdDistance * destinationThresholdDistance);
	}

	public bool IsPowerfulThan(AIBase otherAI)
	{
		return Power > otherAI.Power;
	}

	public bool IsGrounded()
	{
		return IsGroundedAt(selfRigidbody.position);
	}

	public bool IsGroundedAt(Vector2 worldPosition)
	{
		// Bounds * 0.5f gets the extents (half size)
		var groundRaycast = Physics2D.BoxCast(worldPosition, new Vector2(Bounds.x, 0.5f), 0, Vector2.down, (Bounds * 0.5f).y, Layers.Mask.Ground);

		if (groundRaycast.collider)
			return true;

		return false;
	}

	// TODO: This one COULD act like path finder but for V1, it will stay as normal obstacle detector
	/// <summary> Checks whether it can go to that position without any obstacles </summary>
	public bool IsAbleToGoTo(Vector2 worldPosition2D, int layerMask = Layers.Mask.Ground)
	{
		// Check if there is any obstacle in front of self
		var norDirSelfToWorld = (worldPosition2D - selfRigidbody.position).normalized;

		if (Physics2D.BoxCast(selfRigidbody.position, Bounds, 0f, norDirSelfToWorld, Vector2.Distance(selfRigidbody.position, worldPosition2D), layerMask))
			return false;

		return true;
	}

	public virtual void Copy(in AIBase other)
	{
		other.CopyTo(this);
	}

	public virtual void CopyTo(in AIBase main)
	{
		main.Bounds = this.Bounds;
		main.Health = this.Health;
		main.Power = this.Power;
	}


	// Dispose
	public void ReleaseOrDestroySelf()
	{
		if (ParentPool != null)
			ParentPool.Release(this);
		else
			Destroy(this.gameObject);
	}
}


#if UNITY_EDITOR

public abstract partial class AIBase
{
	private void OnDrawGizmosSelected()
	{
		// Display the explosion radius when selected
		Gizmos.color = new Color(1f, 0f, 0f, 0.25f);
		Gizmos.DrawCube(this.transform.position, Bounds);
	}
}

#endif