using UnityEngine;
using UnityEngine.AI;

public abstract partial class AIBase : MonoBehaviour
{
	// Movement
	public float movementVelocity;

	[Tooltip("Set to zero if you dont want any limitation to velocity")]
	public Vector2 maxVelocity;

	[field: SerializeField]
	public NavMeshAgent SelfAgent
	{
		get;
		private set;
	}

	// Target
	public Transform currentTargetToGo;


	// Initialize
	private void Start()
	{
		SelfAgent.updateRotation = false;
		SelfAgent.updateUpAxis = false;
	}


	// Update
	private void FixedUpdate()
	{
		MoveToCurrentTarget();
		LimitSpeed();
	}

	/*private void MoveToCurrentTarget()
	{
		var directionToTarget = (currentTargetToGo.position - this.transform.position);
		SelfRigidbody.velocityX = (movementVelocity * directionToTarget.x);
	}*/

	private void MoveToCurrentTarget()
	{
		SelfAgent.SetDestination(currentTargetToGo.position);
	}

	private void LimitSpeed()
	{
		if (maxVelocity.x != 0 && maxVelocity.y != 0)
			SelfAgent.velocity = Vector3.ClampMagnitude(SelfAgent.velocity, maxVelocity.sqrMagnitude);

		if (maxVelocity.x != 0)
			SelfAgent.velocity = new Vector3(Mathf.Clamp(SelfAgent.velocity.x, -maxVelocity.x, maxVelocity.x), SelfAgent.velocity.y, 0);

		if (maxVelocity.y != 0)
			SelfAgent.velocity = new Vector3(SelfAgent.velocity.x, Mathf.Clamp(SelfAgent.velocity.y, -maxVelocity.y, maxVelocity.y), 0);
	}


	// Dispose
}


#if UNITY_EDITOR

public abstract partial class AIBase
{ }

#endif