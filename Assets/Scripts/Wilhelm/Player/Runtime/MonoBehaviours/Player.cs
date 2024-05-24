using UnityEngine;
using static UnityEngine.InputSystem.InputAction;

[RequireComponent(typeof(Rigidbody2D))]
public sealed partial class Player : MonoBehaviour
{
	private PlayerState _state;

	[SerializeField]
	[Header("Movement")]
	private Rigidbody2D selfRigidbody;

	public float movementVelocity;

	private FRC_Default_InputActions inputActions;

	public PlayerState State
	{
		get => _state;
		set
		{
			if (value != _state)
				StateChanged(value);

			_state = value;
		}
	}


	// Initialize
	private void Awake()
	{
		inputActions = new FRC_Default_InputActions();
		inputActions.Player.Move.performed += OnMoveByInput;
		inputActions.Player.Move.canceled += OnMoveByInput;
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

	private void OnMoveByInput(CallbackContext ctx)
	{
		selfRigidbody.velocityX = (movementVelocity * ctx.ReadValue<Vector2>().x);
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

	private void StateChanged(PlayerState state)
	{ }


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