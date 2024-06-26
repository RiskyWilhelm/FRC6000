using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

// OPTIMIZATION: Needed in future versions
/// <summary> Fresh base of AI. Implements destination system </summary>
/// <remarks> To update allies, you must setup your own collider system with <see cref="OnTriggerStay2DEvent"/> and <see cref="OnTriggerExit2DEvent"/> </remarks>
[RequireComponent(typeof(Rigidbody2D))]
[DisallowMultipleComponent]
public abstract partial class AIBase : MonoBehaviour, ITarget, IPooledObject<AIBase>, ICopyable<AIBase> 
{
	[Header("AIBase Movement")]
	#region AIBase Movement

	[SerializeField]
	protected Rigidbody2D selfRigidbody;

	[SerializeField]
	[Tooltip("You should set this nearly but not same as the collider size")]
	protected Vector2 raycastBounds = new(1f, 1f);

	[NonSerialized]
	private (Vector2? worldDestination, Transform targetDestination, float considerAsReachedDistance) currentDestination;


	#endregion

	[field: Header("AIBase Stats")]
	#region AIBase Stats

	[field: SerializeField]
	public TargetType TargetTag { get; private set; }

	[field: SerializeField]
	public uint Health { get; protected set; }

	[field: SerializeField]
	public uint MaxHealth { get; protected set; }


	#endregion

	[Header("AIBase Target Verify")]
	#region AIBase Target Verify

	public List<TargetType> acceptedTargetTypeList = new();

	public List<TargetType> runawayTargetTypeList = new();

	private readonly Dictionary<Transform, ITarget> cachedNearestTargetDict = new();


	#endregion

	#region AIBase Other

#if UNITY_EDITOR
	[SerializeField]
#else
	[NonSerialized]
#endif
	private PlayerStateType _state;

	protected PlayerStateType State
	{
		get => _state;
		set
		{
			if (value != _state)
			{
				OnStateChanged(value);
				_state = value;
			}
		}
	}

	[field: NonSerialized]
	public IPool<AIBase> ParentPool { get; set; }


#endregion


	// Initialize
	protected virtual void OnEnable()
	{
		ClearDestination();
		State = PlayerStateType.Idle;
		OnStateChanged(State);
	}

	public virtual void OnTakenFromPool(IPool<AIBase> pool)
	{
		RestoreHealth();
	}


	// Update
	protected virtual void Update()
	{
		if (IsReachedToDestination())
		{
			if (currentDestination.worldDestination.HasValue)
				OnReachedToDestination(currentDestination.worldDestination.Value);
			else
				OnReachedToDestination(currentDestination.targetDestination);

			ClearDestination();
		}

		DetectObstacles();
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

	private void DoState()
	{
		switch (State)
		{
			case PlayerStateType.Idle:
			DoIdle();
			break;

			case PlayerStateType.Walking:
			DoWalking();
			break;

			case PlayerStateType.Running:
			DoRunning();
			break;

			case PlayerStateType.Flying:
			DoFlying();
			break;

			case PlayerStateType.Jumping:
			DoJumping();
			break;

			case PlayerStateType.Attacking:
			DoAttacking();
			break;

			case PlayerStateType.Defending:
			DoDefending();
			break;

			case PlayerStateType.Dead:
			DoDead();
			break;
		}
	}

	private void OnStateChanged(PlayerStateType newState)
	{
		switch (newState)
		{
			case PlayerStateType.Idle:
			OnStateChangedToIdle();
			break;

			case PlayerStateType.Walking:
			OnStateChangedToWalking();
			break;

			case PlayerStateType.Running:
			OnStateChangedToRunning();
			break;

			case PlayerStateType.Flying:
			OnStateChangedToFlying();
			break;

			case PlayerStateType.Jumping:
			OnStateChangedToJumping();
			break;

			case PlayerStateType.Attacking:
			OnStateChangedToAttacking();
			break;

			case PlayerStateType.Defending:
			OnStateChangedToDefending();
			break;

			case PlayerStateType.Dead:
			OnStateChangedToDead();
			break;
		}
	}

	protected virtual void DoIdle()
	{ }

	protected virtual void DoWalking()
	{ }

	protected virtual void DoRunning()
	{ }

	protected virtual void DoFlying()
	{ }

	protected virtual void DoJumping()
	{ }

	protected virtual void DoAttacking()
	{ }
	
	protected virtual void DoDefending()
	{ }

	protected virtual void DoDead()
	{ }

	protected virtual void OnStateChangedToIdle()
	{ }

	protected virtual void OnStateChangedToWalking()
	{ }

	protected virtual void OnStateChangedToRunning()
	{ }

	protected virtual void OnStateChangedToFlying()
	{ }

	protected virtual void OnStateChangedToJumping()
	{ }

	protected virtual void OnStateChangedToAttacking()
	{ }

	protected virtual void OnStateChangedToDefending()
	{ }

	protected virtual void OnStateChangedToDead()
	{ }

	public void TakeDamage(uint damage, Vector2 occuredWorldPosition)
	{
		// Check for System.OverflowException. This is because the health may become negative and this is unwanted behaviour
		try
		{
			Health = checked(Health - damage);
		}
		catch
		{
			Health = 0;
		}
		finally
		{
			if ((this as ITarget).IsDead)
				State = PlayerStateType.Dead;
		}
	}

	protected void RestoreHealth()
	{
		Health = MaxHealth;
	}

	protected virtual void OnChangedDestination(Vector2? newDestination)
	{ }

	protected virtual void OnReachedToDestination(Transform reachedDestinationTarget)
	{ }

	protected virtual void OnReachedToDestination(Vector2 reachedDestination)
	{ }

	/// <summary> Verifies the <paramref name="target"/> position and sets destination if succeeded </summary>
	public bool TrySetDestinationTo(Transform target, float considerAsReachedDistance = 1f)
	{
		// Check if self already reached to or cant go to the newDestination, then clear it
		if (IsReachedTo(target) || !IsAbleToGoTo(target))
			return false;

		// Set the new destination
		SetDestinationTo(target, considerAsReachedDistance);
		return true;
	}

	/// <summary> Verifies the <paramref name="newDestination"/> and sets destination if succeeded </summary>
	public bool TrySetDestinationTo(Vector2 newDestination, float considerAsReachedDistance = 1f)
	{
		// Check if self already reached to or cant go to the newDestination, then clear it
		if (IsReachedTo(newDestination) || !IsAbleToGoTo(newDestination))
			return false;

		// Set the new destination
		SetDestinationTo(newDestination, considerAsReachedDistance);
		return true;
	}

	/// <remarks> Use if you verified the <paramref name="target"/> position already </remarks>
	public void SetDestinationTo(Transform target, float considerAsReachedDistance = 1f)
	{
		currentDestination.worldDestination = null;
		currentDestination.targetDestination = target;
		currentDestination.considerAsReachedDistance = considerAsReachedDistance;

		OnChangedDestination(target.position);
	}

	/// <remarks> Use if you verified the <paramref name="newDestination"/> already </remarks>
	public void SetDestinationTo(Vector2 newDestination, float considerAsReachedDistance = 1f)
	{
		currentDestination.worldDestination = newDestination;
		currentDestination.targetDestination = null;
		currentDestination.considerAsReachedDistance = considerAsReachedDistance;

		OnChangedDestination(newDestination);
	}
	
	public void ClearDestination()
	{
		currentDestination.worldDestination = null;
		currentDestination.targetDestination = null;
		currentDestination.considerAsReachedDistance = 0;
		OnChangedDestination(null);
	}

	public bool TryGetDestination(out Vector2 worldDestination)
	{
		worldDestination = Vector2.zero;

		if (currentDestination.worldDestination.HasValue)
		{
			worldDestination = currentDestination.worldDestination.Value;
			return true;
		}
		else if (TryGetDestinationTarget(out Transform destinationTarget))
		{
			worldDestination = destinationTarget.position;
			return true;
		}

		return false;
	}

	public bool TryGetDestinationTarget(out Transform worldDestinationTarget)
	{
		worldDestinationTarget = null;

		if (currentDestination.targetDestination && currentDestination.targetDestination.gameObject.activeSelf)
			worldDestinationTarget = currentDestination.targetDestination;

		return worldDestinationTarget != null;
	}

	public bool TrySetDestinationAwayFrom(Transform target, float considerAsReachedDistance = 1f, float checkInDegree = 180f, float checkEveryDegree = (180 / 12), bool isGroundedOnly = false)
		=> TrySetDestinationAwayFrom(target.position, considerAsReachedDistance, checkInDegree, checkEveryDegree, isGroundedOnly);

	/// <param name="isGroundedOnly"> Get the grounded direction only </param>
	public bool TrySetDestinationAwayFrom(Vector2 target, float considerAsReachedDistance = 1f, float checkInDegree = 180f, float checkEveryDegree = (180 / 12), bool isGroundedOnly = false)
	{
		var isDestinationSet = false;

		// Prevent from no check by keeping the values positive
		// Clamp checkInAngle and checkInEveryDegree to 360
		checkInDegree = MathF.Abs(checkInDegree) % 360f;
		checkEveryDegree = Math.Clamp(MathF.Abs(checkEveryDegree) % checkInDegree, 1f, 360f);

		// We will work with radians to prevent unnecessary conversion so convert the method parameters to radians
		var radCheckInDegree = checkInDegree * Mathf.Deg2Rad;
		var radCheckEveryDegree = checkEveryDegree * Mathf.Deg2Rad;

		// Get direction of 'target to self' and its angle in radians
		var norDirTargetToSelf = (selfRigidbody.position - target).normalized;
		var radTargetToSelfAngle = norDirTargetToSelf.ToAngle_360();

		// Get starting and ending point in radians
		var radStartAngle = radTargetToSelfAngle - (radCheckInDegree * 0.5f);
		var radEndAngle = radTargetToSelfAngle + (radCheckInDegree * 0.5f);
		var radCurrentAngle = radStartAngle;

		// Ready the list for storing the look directions. ValueTuple<float, Vector2> stores <DotProductLookDir, WorldPositionDestination>
		var lookDirPooledList = ListPool<ValueTuple<float, Vector2>>.Get();

		// Do cast for every radCheckForEveryDegree and store valid one(s) look direction to check which angle is close enough to the norDirTargetToSelf direction
		while (radCurrentAngle <= radEndAngle)
		{
			// nor stands for "normalized". Multiplying the Bounds with random value because there should be no conflict between Bounds'es
			var norCurrentAngle = VectorUtils.AngleToVector(radCurrentAngle);
			var newDestination = selfRigidbody.position + (norCurrentAngle * (raycastBounds * 3));

			// If nothing is on the way, add to look directions based on grounded state on newDestination
			if (IsAbleToGoTo(newDestination))
			{
				var newLookDirTuple = new ValueTuple<float, Vector2>(Vector2.Dot(norDirTargetToSelf, norCurrentAngle), newDestination);

				// If direction only accepts the grounded, check it
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

		// Select the one which is close enough to pointing the same way as norDirTargetToSelf direction
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
			SetDestinationTo(closestNorDirTuple.Item2, considerAsReachedDistance);
			isDestinationSet = true;
		}

		// DEBUG
		/*Debug.DrawLine(selfRigidbody.position, selfRigidbody.position + new Vector2(MathF.Cos(radTargetToSelfAngle), MathF.Sin(radTargetToSelfAngle)));
		Debug.DrawLine(selfRigidbody.position, selfRigidbody.position + new Vector2(MathF.Cos(radStartAngle), MathF.Sin(radStartAngle)), Color.cyan);
		Debug.DrawLine(selfRigidbody.position, selfRigidbody.position + new Vector2(MathF.Cos(radEndAngle), MathF.Sin(radEndAngle)), Color.red);*/

		// Dispose the list
		ListPool<ValueTuple<float, Vector2>>.Release(lookDirPooledList);
		return isDestinationSet;
	}

	public bool TrySetDestinationToNearestIn(IEnumerable<Transform> transformEnumerable, float considerAsReachedDistance = 1f)
	{
		if (this.transform.TryGetNearestTransform(transformEnumerable, out Transform nearestTransform,
			(iteratedTransform) => IsAbleToGoTo(iteratedTransform)))
		{
			SetDestinationTo(nearestTransform, considerAsReachedDistance);
			return true;
		}

		return false;
	}

	public bool TrySetDestinationToNearestIn(IEnumerable<Vector2> positionEnumerable, float considerAsReachedDistance = 1f)
	{
		if (VectorExtensions.TryGetNearestVector(this.transform.position, positionEnumerable, out Vector2 nearestPosiiton,
			(iteratedPosition) => IsAbleToGoTo(iteratedPosition)))
		{
			SetDestinationTo(nearestPosiiton, considerAsReachedDistance);
			return true;
		}

		return false;
	}

	public bool TrySetDestinationToNearestIn(IEnumerable<ITarget> targetEnumerable, float considerAsReachedDistance = 1f)
	{
		if (TryGetNearestChaseableTargetIn(targetEnumerable, out ITarget nearestTarget, out _))
		{
			SetDestinationTo((nearestTarget as Component).transform, considerAsReachedDistance);
			return true;
		}

		return false;
	}

	/// <param name="nearestTargetAccessDict"> Stores all current nearest targets. Do not use outside of the method call </param>
	public bool TryGetNearestChaseableTargetIn(IEnumerable<ITarget> targetEnumerable, out ITarget nearestTarget, out Dictionary<Transform, ITarget> nearestTargetAccessDict, Predicate<Transform> predicateNearest = null)
	{
		nearestTargetAccessDict = cachedNearestTargetDict;
		nearestTarget = null;

		// Copy the enemyInRangeDict to the pooled dictionary while taking the enemy's transform
		foreach (var iteratedTarget in targetEnumerable)
			cachedNearestTargetDict.TryAdd((iteratedTarget as Component).transform, iteratedTarget);

		// Get nearest target
		if (this.transform.TryGetNearestTransform(cachedNearestTargetDict.Keys, out Transform nearestTransform,
			predicateNearest
			?? ((iteratedTarget) => (acceptedTargetTypeList.Contains(cachedNearestTargetDict[iteratedTarget].TargetTag) && IsAbleToGoTo(iteratedTarget)))))
		{
			nearestTarget = cachedNearestTargetDict[nearestTransform];
		}

		cachedNearestTargetDict.Clear();
		return nearestTarget != null;
	}

	public bool TrySetDestinationAwayFromNearestIn(IEnumerable<Transform> targetEnumerable, float considerAsReachedDistance = 1f, float checkInDegree = 180f, float checkEveryDegree = (180 / 12), bool isGroundedOnly = false)
	{
		if (this.transform.TryGetNearestTransform(targetEnumerable, out Transform nearestTransform,
			(iteratedTransform) => IsAbleToGoTo(iteratedTransform)))
			if (TrySetDestinationAwayFrom(nearestTransform, considerAsReachedDistance, checkInDegree, checkEveryDegree, isGroundedOnly))
				return true;

		return false;
	}

	public bool TrySetDestinationAwayFromNearestIn(IEnumerable<Vector2> vectorEnumerable, float considerAsReachedDistance = 1f, float checkInDegree = 180f, float checkEveryDegree = (180 / 12), bool isGroundedOnly = false)
	{
		if (VectorExtensions.TryGetNearestVector(this.transform.position, vectorEnumerable, out Vector2 nearestVector,
			(iteratedPosition) => IsAbleToGoTo(iteratedPosition)))
			if (TrySetDestinationAwayFrom(nearestVector, considerAsReachedDistance, checkInDegree, checkEveryDegree, isGroundedOnly))
				return true;

		return false;
	}

	public bool TrySetDestinationAwayFromNearestIn(IEnumerable<ITarget> targetEnumerable, float considerAsReachedDistance = 1f, float checkInDegree = 180f, float checkEveryDegree = (180 / 12), bool isGroundedOnly = false)
	{
		if (TryGetNearestChaseableTargetIn(targetEnumerable, out var nearestTarget, out _,
			(iteratedTarget) => (runawayTargetTypeList.Contains(cachedNearestTargetDict[iteratedTarget].TargetTag) && IsAbleToGoTo(iteratedTarget))))
		{
			// TODO: What if nearestTarget is not a Component?
			if (TrySetDestinationAwayFrom((nearestTarget as Component).transform, considerAsReachedDistance, checkInDegree, checkEveryDegree, isGroundedOnly))
				return true;
		}

		return false;
	}

	/// <remarks> Returns true when there is no destination </remarks>
	public bool IsReachedToDestination()
	{
		if (!TryGetDestination(out Vector2 destination))
			return true;

		return IsReachedTo(destination, currentDestination.considerAsReachedDistance);
	}

	public bool IsReachedTo(Transform target, float considerAsReachedDistance = 1f)
		=> IsReachedTo(target.position, considerAsReachedDistance);

	public bool IsReachedTo(Vector2 worldPosition, float considerAsReachedDistance = 1f)
	{
		// If self position equals or inside the threshold of destination, consider as reached
		var distSelfToDestination = (worldPosition - selfRigidbody.position);
		return (distSelfToDestination.sqrMagnitude <= (considerAsReachedDistance * considerAsReachedDistance));
	}

	public bool IsGrounded()
		=> IsGroundedAt(selfRigidbody.position);

	public bool IsGroundedAt(Vector2 worldPosition)
	{
		// Bounds * 0.5f gets the extents (half size)
		var groundRaycast = Physics2D.BoxCast(worldPosition, new Vector2(raycastBounds.x, 0.5f), 0, Vector2.down, (raycastBounds * 0.5f).y, Layers.Mask.Ground);

		if (groundRaycast.collider)
			return true;

		return false;
	}

	/// <inheritdoc cref="IsAbleToGoTo(Vector2, int)"/>
	protected bool IsAbleToGoTo(Transform target, int layerMask = Layers.Mask.Ground)
		=> IsAbleToGoTo(target.position, layerMask);

	// TODO: This one COULD act like path finder but for V1, it will stay as normal obstacle detector
	/// <summary> Verifies position whether self can go without any obstacles </summary>
	protected bool IsAbleToGoTo(Vector2 worldPosition2D, int layerMask = Layers.Mask.Ground)
	{
		// Check if there is any obstacle in front of self
		var norDirSelfToWorld = (worldPosition2D - selfRigidbody.position).normalized;

		if (Physics2D.BoxCast(selfRigidbody.position, raycastBounds, 0f, norDirSelfToWorld, Vector2.Distance(selfRigidbody.position, worldPosition2D), layerMask))
			return false;

		return true;
	}

	public virtual void Copy(in AIBase other)
	{
		other.CopyTo(this);
	}

	public virtual void CopyTo(in AIBase main)
	{
		main.raycastBounds = this.raycastBounds;
		main.Health = this.Health;
		main.MaxHealth = this.MaxHealth;
		main.acceptedTargetTypeList = new List<TargetType>(this.acceptedTargetTypeList);
		main.runawayTargetTypeList = new List<TargetType>(this.runawayTargetTypeList);
	}


	// Dispose
	public void ReleaseOrDestroySelf()
	{
		if (ParentPool != null)
			ParentPool.Release(this);
		else
			Destroy(this.gameObject);
	}

	public virtual void OnReleaseToPool(IPool<AIBase> pool)
	{ }
}


#if UNITY_EDITOR

public abstract partial class AIBase
{
	protected virtual void OnDrawGizmosSelected()
	{
		DrawRaycastBoundsGizmos();
		DrawDestinationGizmos();
	}

	private void DrawDestinationGizmos()
	{
		if (TryGetDestination(out Vector2 worldDestination))
		{
			Gizmos.color = new Color(0f, 1f, 0f, 0.25f);
			Gizmos.DrawSphere(worldDestination, currentDestination.considerAsReachedDistance);
		}
	}

	private void DrawRaycastBoundsGizmos()
	{
		Gizmos.color = new Color(1f, 0f, 0f, 0.25f);
		Gizmos.DrawCube(this.transform.position, raycastBounds);
	}
}

#endif