using System;
using System.Collections.Generic;
using UnityEngine;

// OPTIMIZATION: Needed in future versions
/// <summary> Fresh base of AI. Implements destination system </summary>
[RequireComponent(typeof(Rigidbody2D))]
[DisallowMultipleComponent]
public abstract partial class AIBase : StateMachineDrivenPlayerBase, ITarget, IPooledObject<AIBase>, ICopyable<AIBase> 
{
	[Header("AIBase Movement")]
	#region AIBase Movement

	[NonSerialized]
	private (Vector2? worldDestination, Transform targetDestination, float considerAsReachedDistance) currentDestinationTuple;


	#endregion

	[field: Header("AIBase Stats")]
	#region AIBase Stats

	[field: SerializeField]
	public uint Health { get; protected set; }

	[field: SerializeField]
	public uint MaxHealth { get; protected set; }


	#endregion

	[field: Header("AIBase Target")]
	#region AIBase Target

	[field: SerializeField]
	public TargetType TargetTag { get; private set; }

	public List<TargetType> acceptedTargetTypeList = new();

	public List<TargetType> runawayTargetTypeList = new();


	#endregion

	#region AIBase Other

	[field: NonSerialized]
	public IPool<AIBase> ParentPool { get; set; }


	#endregion


	// Initialize
	public virtual void OnTakenFromPool(IPool<AIBase> pool)
	{
		RestoreHealth();
	}


	// Update
	protected override void Update()
	{
		CheckDestinationState();
		DetectObstacles();
		base.Update();
	}

	protected void DetectObstacles()
	{
		// If not able to go to next position and destination, clear destination and set position to last position. Useful for where player cant fall to bottom from an edge and obstacle detection
		var nextPredictedPosition = selfRigidbody.position + (selfRigidbody.velocity * Time.deltaTime);
		var hasDestination = TryGetDestination(out Vector2 worldDestination);

		if (!IsAbleToGoToVector(nextPredictedPosition) || (hasDestination && !IsAbleToGoToVector(worldDestination)))
			ClearDestination();
	}

	public virtual void TakeDamage(uint damage, Vector2 occuredWorldPosition)
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

	protected virtual void RestoreHealth()
	{
		Health = MaxHealth;
	}

	public bool TryGetDestinationTransfom(out Transform destinationTarget)
	{
		destinationTarget = null;

		if (currentDestinationTuple.targetDestination && currentDestinationTuple.targetDestination.gameObject.activeSelf)
			destinationTarget = currentDestinationTuple.targetDestination;

		return destinationTarget;
	}

	public bool TryGetDestinationVector(out Vector2 worldDestination)
	{
		worldDestination = Vector2.zero;

		if (currentDestinationTuple.worldDestination.HasValue)
		{
			worldDestination = currentDestinationTuple.worldDestination.Value;
			return true;
		}

		return false;
	}

	public bool TryGetDestination(out Vector2 worldDestination)
	{
		if (TryGetDestinationVector(out worldDestination))
			return true;
		else if (TryGetDestinationTransfom(out Transform destinationTransform))
		{
			worldDestination = destinationTransform.position;
			return true;
		}

		return false;
	}

	public void SetDestinationToTransform(Transform targetDestination, float considerAsReachedDistance = 0.25f)
	{
		currentDestinationTuple.worldDestination = null;
		currentDestinationTuple.targetDestination = targetDestination;
		currentDestinationTuple.considerAsReachedDistance = considerAsReachedDistance;
		OnChangedDestination(targetDestination.position);
	}

	public void SetDestinationToVector(Vector2 newDestination, float considerAsReachedDistance = 1f)
	{
		currentDestinationTuple.worldDestination = newDestination;
		currentDestinationTuple.targetDestination = null;
		currentDestinationTuple.considerAsReachedDistance = considerAsReachedDistance;
		OnChangedDestination(newDestination);
	}

	/// <summary> Verifies the <paramref name="targetDestination"/> position and sets destination if succeeded </summary>
	public bool TrySetDestinationToTransform(Transform targetDestination, float considerAsReachedDistance = 1f)
	{
		// Check if self already reached to or cant go to the newDestination, then clear it
		if (IsReachedToVector(targetDestination.position) || !IsAbleToGoToVector(targetDestination.position))
			return false;

		// Set the new destination
		SetDestinationToTransform(targetDestination, considerAsReachedDistance);
		return true;
	}

	/// <summary> Verifies the <paramref name="newDestination"/> and sets destination if succeeded </summary>
	public bool TrySetDestinationToVector(Vector2 newDestination, float considerAsReachedDistance = 1f)
	{
		// Check if self already reached to or cant go to the newDestination, then clear it
		if (IsReachedToVector(newDestination) || !IsAbleToGoToVector(newDestination))
			return false;

		// Set the new destination
		SetDestinationToVector(newDestination, considerAsReachedDistance);
		return true;
	}

	// TODO: Create transform version to set away constantly and refactor this method
	public bool TrySetDestinationAwayFromVector(Vector2 targetWorldPosition, float considerAsReachedDistance = 1f, float checkInDegree = 180f, float checkEveryDegree = (180 / 12), bool isGroundedOnly = false)
	{
		var isFoundValidAngle = false;

		// Clamp values to be safe
		checkInDegree = MathF.Abs(checkInDegree) % 360f;
		checkEveryDegree = Math.Clamp(MathF.Abs(checkEveryDegree) % checkInDegree, 1f, 360f);

		// Get "target to self" angle
		var norDirTargetToSelf = (selfRigidbody.position - targetWorldPosition).normalized;
		var degTargetToSelfAngle = norDirTargetToSelf.ToDegreeAngle_360();

		// Get starting and ending point
		var degStartAngle = degTargetToSelfAngle - (checkInDegree * 0.5f);
		var degEndAngle = degTargetToSelfAngle + (checkInDegree * 0.5f);
		var degCurrentAngle = degStartAngle;

		// Do cast for every check angle and store valid one(s) look direction to check which angle is close enough to the "target to self" direction
		(float lookDirDotProduct, Vector2 worldDestination) closestLookDirTuple = (-1, Vector2.zero);

		while (degCurrentAngle <= degEndAngle)
		{
			var iteratedAngleWorldDestination = selfRigidbody.position + (VectorUtils.DegreeToNormalizedVector(degCurrentAngle) * (raycastBounds * 3));
			var iteratedAngleLookDirDotProduct = Vector2.Dot(norDirTargetToSelf, VectorUtils.DegreeToNormalizedVector(degCurrentAngle));

			// DEBUG
			Debug.DrawLine(selfRigidbody.position, iteratedAngleWorldDestination, new Color(0.75f, 0.75f, 0.75f, 0.5f));

			// Check angle destination and it's look dir
			if (IsAbleToGoToVector(iteratedAngleWorldDestination))
			{
				if (isGroundedOnly && !IsGroundedAtVector(iteratedAngleWorldDestination))
				{
					degCurrentAngle += checkEveryDegree;
					continue;
				}

				if (iteratedAngleLookDirDotProduct > closestLookDirTuple.lookDirDotProduct)
					closestLookDirTuple = (iteratedAngleLookDirDotProduct, iteratedAngleWorldDestination);

				isFoundValidAngle = true;
			}
			
			degCurrentAngle += checkEveryDegree;
		}

		// DEBUG
		if (isFoundValidAngle)
		{
			Debug.DrawLine(selfRigidbody.position, closestLookDirTuple.worldDestination, Color.green);
			SetDestinationToVector(closestLookDirTuple.worldDestination, considerAsReachedDistance);
		}

		return isFoundValidAngle;
	}

	public void ClearDestination()
	{
		currentDestinationTuple.worldDestination = null;
		currentDestinationTuple.targetDestination = null;
		currentDestinationTuple.considerAsReachedDistance = 0;
		OnChangedDestination(null);
	}

	protected void CheckDestinationState()
	{
		if (IsReachedToDestinationOrNotSet())
		{
			if (TryGetDestinationVector(out Vector2 worldDestination))
				OnReachedToDestinationVector(worldDestination);
			else if (TryGetDestinationTransfom(out Transform worldDestinationTarget))
				OnReachedToDestinationTransform(worldDestinationTarget);

			ClearDestination();
		}
	}

	protected virtual void OnChangedDestination(Vector2? newDestination)
	{ }

	protected virtual void OnReachedToDestinationTransform(Transform reachedDestinationTarget)
	{ }

	protected virtual void OnReachedToDestinationVector(Vector2 reachedDestination)
	{ }
	
	public bool IsReachedToDestinationOrNotSet()
	{
		if (!TryGetDestination(out Vector2 worldDestination))
			return true;

		return IsReachedToVector(worldDestination, currentDestinationTuple.considerAsReachedDistance);
	}

	public bool IsReachedToVector(Vector2 worldPosition, float considerAsReachedDistance = 1f)
	{
		// If self position equals or inside the threshold of destination, consider as reached
		var distSelfToDestination = (worldPosition - selfRigidbody.position);
		return (distSelfToDestination.sqrMagnitude <= (considerAsReachedDistance * considerAsReachedDistance));
	}

	// TODO: Un-finished and abondoned. Pathfinding requires a better way to handle such as via Checkpoints or A* pathfinding algorithm or navmesh 2D
	public virtual bool IsAbleToGoToVector(Vector2 worldPosition2D, int layerMask = Layers.Mask.Ground)
	{
		// Check if there is any obstacle in front of self
		var dirSelfToTarget = (worldPosition2D - selfRigidbody.position);

		/*if (Physics2D.BoxCast(selfRigidbody.position, raycastBounds, 0f, norDirSelfToTarget, Vector2.Distance(selfRigidbody.position, worldPosition2D), layerMask))
			return false;*/

		if (Physics2D.Linecast(selfRigidbody.position, selfRigidbody.position + dirSelfToTarget, layerMask))
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
	protected virtual void OnDisable()
	{
		ClearDestination();
	}

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
			Gizmos.DrawSphere(worldDestination, currentDestinationTuple.considerAsReachedDistance);
		}
	}

	private void DrawRaycastBoundsGizmos()
	{
		Gizmos.color = new Color(1f, 0f, 0f, 0.25f);
		Gizmos.DrawCube(this.transform.position, raycastBounds);
	}
}

#endif