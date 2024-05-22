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
		MoveByInputDirectionVelocity();
	}

	private void MoveByInputDirectionVelocity()
	{
		SelfRigidbody.velocityX = (movementVelocity * Time.deltaTime * inputDirection.x);
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