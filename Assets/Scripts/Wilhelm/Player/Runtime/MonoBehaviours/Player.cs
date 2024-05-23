using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public sealed partial class Player : MonoBehaviour
{
	[Header("Movement")]
	[SerializeField]
	private Rigidbody2D selfRigidbody;

	public float movementVelocity;

	private Vector2 inputDirection;

	private FRC_Default_InputActions inputActions;

	public PlayerState State { get; private set; }


	// Initialize
	private void Awake()
	{
		inputActions = new FRC_Default_InputActions();
		inputActions.Player.Move.performed += (ctx) => inputDirection = ctx.ReadValue<Vector2>();
		inputActions.Player.Move.canceled += (ctx) => inputDirection = Vector2.zero;
	}

	private void OnEnable()
	{
		inputActions.Player.Enable();
	}


	// Update
	private void Update()
	{
		UpdateState();
	}

	private void FixedUpdate()
	{
		MoveByInputDirectionVelocity();
	}

	private void MoveByInputDirectionVelocity()
	{
		selfRigidbody.velocityX = (movementVelocity * inputDirection.x);
	}

	private void UpdateState()
	{
		// Do not do magnitude check. Magnitude uses Mathf.Sqrt at the backend which is an heavy operation
		if (selfRigidbody.velocity == Vector2.zero)
			State = PlayerState.Idle;
		else
			State = PlayerState.Running;

		// Extendable to the jump, fly, falling or whatever
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