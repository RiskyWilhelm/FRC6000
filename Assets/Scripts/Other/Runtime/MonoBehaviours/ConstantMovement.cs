using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public sealed partial class ConstantMovement : MonoBehaviour
{
	#region ConstantMovement Movement

	[SerializeField]
    private Rigidbody2D selfRigidbody;

    [SerializeField]
    private Vector2 velocity;

    [SerializeField]
    [Tooltip("If no limitation wanted for axis, set to zero")]
    private Vector2 maxVelocity;

	[SerializeField]
	private UpdateType updateType;


	#endregion


	// Update
	private void Update()
	{
		if (updateType is UpdateType.Update)
			UpdateVelocity();
	}

	private void FixedUpdate()
	{
		if (updateType is UpdateType.FixedUpdate)
			UpdateVelocity();
	}

	private void LateUpdate()
	{
		if (updateType is UpdateType.LateUpdate)
			UpdateVelocity();
	}

	private void UpdateVelocity()
	{
		selfRigidbody.velocity = velocity;
		LimitVelocity();
	}

	private void LimitVelocity()
	{
		if (maxVelocity.x > 0)
			selfRigidbody.velocityX = maxVelocity.x;

		if (maxVelocity.y > 0)
			selfRigidbody.velocityY = maxVelocity.y;
	}
}


#if UNITY_EDITOR

public sealed partial class ConstantMovement
{ }

#endif