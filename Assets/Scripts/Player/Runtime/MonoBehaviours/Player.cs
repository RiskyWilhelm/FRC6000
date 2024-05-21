using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
public sealed partial class Player : MonoBehaviour
{
	// Actions
	private FRC_Default_InputActions inputActions;

	// Movement
	private Vector2 inputDirection;

	public float movementVelocity;

	[Tooltip("Set to zero if you dont want any limitation to velocity")]
	public Vector2 maxVelocity;

	[field: SerializeField]
	public Rigidbody2D SelfRigidbody
	{
		get;
		private set;
	}


	// Initialize
	private void Awake()
	{
		inputActions = new FRC_Default_InputActions();
		inputActions.Player.Move.performed += MoveActionPerformed;
		inputActions.Player.Move.canceled += MoveActionCanceled;
	}

	private void OnEnable()
	{
		inputActions.Player.Enable();
	}

	private void MoveActionPerformed(InputAction.CallbackContext callbackContext)
	{
		inputDirection = callbackContext.ReadValue<Vector2>();
	}

	private void MoveActionCanceled(InputAction.CallbackContext callbackContext)
	{
		inputDirection = Vector2.zero;
	}


	// Update
	private void FixedUpdate()
	{
		MoveByInputDirection();
		LimitSpeed();
	}

	private void MoveByInputDirection()
	{
		SelfRigidbody.velocityX = (movementVelocity * inputDirection.x);
	}

	private void LimitSpeed()
	{
		if (maxVelocity.x != 0)
			SelfRigidbody.velocityX = Mathf.Clamp(SelfRigidbody.velocity.x, -maxVelocity.x, maxVelocity.x);

		if (maxVelocity.y != 0)
			SelfRigidbody.velocityY = Mathf.Clamp(SelfRigidbody.velocity.y, -maxVelocity.y, maxVelocity.y);
	}
	
	// Dispose
	private void OnDisable()
	{
		inputActions.Player.Disable();
	}
}


#if UNITY_EDITOR

public sealed partial class Player
{ }

#endif