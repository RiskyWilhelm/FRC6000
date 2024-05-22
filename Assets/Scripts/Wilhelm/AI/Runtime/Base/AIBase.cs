using AYellowpaper;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[DisallowMultipleComponent]
public abstract partial class AIBase : MonoBehaviour, IAITarget
{
	// Movement
	public virtual Vector3 Position => this.transform.position;

	public float horizontalVelocity;

	private float overrideHorizontalVelocity;

	[field: SerializeField]
	public Rigidbody2D SelfRigidbody
	{
		get;
		private set;
	}

	public AIState State
	{
		get;
		private set;
	}

	// Target
	public string[] allowedTags = new string[0];

	public InterfaceReference<IAITarget> currentTarget;

	[field: SerializeField]
	public virtual byte Power
	{
		get;
		private set;
	}

	[field: SerializeField]
	public float OthersMaxApproachDistance
	{
		get;
		private set;
	}


	// Update
	protected virtual void Update()
	{
		DoState();
	}

	protected virtual void FixedUpdate()
	{
		MoveByVelocity();
	}

	public virtual void OnGotCaughtBy(AIBase chaser)
	{ }

	public void OnCaughtSomething(Collider2D collider)
	{
		Debug.LogFormat(collider, "OnCaughtSomething: ", collider.name);
		// Check if event wants to reflect the collision. If there is no EventReflector, it is the main object that wants the event
		if (!EventReflector.TryGetReflectedGameObject(collider.gameObject, out GameObject colliderGameObject))
			colliderGameObject = collider.gameObject;

		// Check if this(self) can catch the detected one (AITargetDummy tag is passing always)
		bool isAbleToCatch = false;
		if (!colliderGameObject.CompareTag(Tags.AITargetDummy))
		{
			foreach (var iteratedTag in allowedTags)
			{
				if (colliderGameObject.CompareTag(iteratedTag))
					isAbleToCatch = true;
			}

			if (!isAbleToCatch)
				return;
		}

		// Try to catch
        if (colliderGameObject.TryGetComponent<IAITarget>(out IAITarget foundTarget))
			foundTarget.OnGotCaughtBy(this);
	}

	protected virtual void DoState()
	{
		UpdateState();
		switch (State)
		{
			case AIState.Idle:
				if (currentTarget.UnderlyingValue == null)
					DoIdle();
			break;

			case AIState.RunningAway:
				goto case AIState.Idle;

			case AIState.Chasing:
				DoChasing();
			break;
		}
	}

	protected virtual void UpdateState()
	{
		// If there is a target, try chasing or runaway
		if (currentTarget.UnderlyingValue != null)
		{
			// Cancel Idle
			//CancelInvoke(nameof(DoIdleMovement));

			// Update to other states
			if (currentTarget.Value.IsChaseableBy(this))
				State = AIState.Chasing;
			else
				State = AIState.RunningAway;
		}
		else
			State = AIState.Idle;
	}

	protected virtual void DoIdle()
	{
		// Initialize random position
		var horizontalRandomPosition = this.transform.position;
		horizontalRandomPosition.x += Random.Range(-3, 3);

		// Initialize dummy
		var dummy = AITargetDummyPool.Get(horizontalRandomPosition);
		dummy.ownerAI = this;

		currentTarget.UnderlyingValue = dummy;
	}

	protected void DoChasing()
	{
		if (currentTarget.UnderlyingValue == null)
			return;

		var distanceToTarget = (currentTarget.Value.Position - this.transform.position);

		if (distanceToTarget.x >= currentTarget.Value.OthersMaxApproachDistance)
			overrideHorizontalVelocity = horizontalVelocity;
		else if (distanceToTarget.x <= -currentTarget.Value.OthersMaxApproachDistance)
			overrideHorizontalVelocity = -horizontalVelocity;
		else
			overrideHorizontalVelocity = 0;
	}

	protected void MoveByVelocity()
	{
		SelfRigidbody.velocityX = overrideHorizontalVelocity * Time.deltaTime;
	}
}


#if UNITY_EDITOR

public abstract partial class AIBase
{ }

#endif