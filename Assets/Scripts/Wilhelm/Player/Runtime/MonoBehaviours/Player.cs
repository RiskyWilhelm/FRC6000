using System;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.InputSystem.InputAction;

[RequireComponent(typeof(Rigidbody2D))]
[DisallowMultipleComponent]
public sealed partial class Player : StateMachineDrivenPlayerBase, IInteractor, ICarrier, IFrameDependentPhysicsInteractor<PlayerPhysicsInteractionType>
{
	[Header("Player Walking")]
	#region Player Walking

	[SerializeField]
	private uint walkingForce;

	[SerializeField]
	[Tooltip("If you want single axis limited only, set other axis to zero")]
	private Vector2 walkingMaxVelocity;


	#endregion

	[Header("Player Running")]
	#region Player Running

	[SerializeField]
	private uint runningForce;

	[SerializeField]
	[Tooltip("If you want single axis limited only, set other axis to zero")]
	private Vector2 runningMaxVelocity;

	private bool isRunningActivated;


	#endregion

	[Header("Player Jumping")]
	#region Player Jumping

	[SerializeField]
	[Tooltip("Decides when it should switch to idle or flying")]
	private Timer jumpingReleaseStateTimer = new(0.75f);

	[SerializeField]
	[Tooltip("Sticks the state to jumping")]
	private Timer jumpingBlockStateTimer = new(0.25f);

	[SerializeField]
	private uint jumpingForce;

	[SerializeField]
	[Range(0, 360)]
	[Tooltip("Decides what angle should be considered as jumpable")]
	private float jumpingAngle = 45f;


	#endregion

	[Header("Player Carry")]
	#region Player Carry

	[SerializeField]
	private Transform defaultCarryPoint;

	[NonSerialized]
	private ICarryableValue<Rigidbody2D> currentCarried;


	#endregion

	#region Player Interaction

	[NonSerialized]
	public bool isInteractionBlocked;

	[NonSerialized]
	private readonly List<IInteractableValue<Transform>> interactablesInRangeList = new();


	#endregion

	#region Other

	[NonSerialized]
	private FRC_Default_InputActions inputActions;

	[NonSerialized]
	private sbyte norDirHorizontalInput;

	[NonSerialized]
	private readonly Queue<(PlayerPhysicsInteractionType triggerType, Collider2D collider2D, Collision2D collision2D)> physicsInteractionQueue = new();


	#endregion


	// Initialize
	private void Awake()
	{
		inputActions = new FRC_Default_InputActions();
		inputActions.Player.Move.performed += OnInputMovePerformed;
		inputActions.Player.Move.canceled += OnInputMoveCanceled;

		inputActions.Player.Sprint.performed += OnInputSprintPerformed;
		inputActions.Player.Sprint.canceled += OnInputSprintCanceled;

		inputActions.Player.Jump.performed += OnInputJumpPerformed;

		inputActions.Player.Interact.performed += OnInputInteractPerformed;
	}

	protected override void OnEnable()
	{
		inputActions.Player.Enable();
		base.OnEnable();
	}


	// Update
	protected override void Update()
	{
		DoFrameDependentPhysics();
		DoCarried();
		base.Update();
	}

	
	public void Carry(ICarryable carryable, Rigidbody2D carryableRigidbody)
	{
		currentCarried = new (carryable, carryableRigidbody);
		carryable.OnCarried(this);
	}

	public void StopCarrying()
	{
		currentCarried.carryable?.OnUncarried(this);
		currentCarried = default;
	}

	public void StopCarrying(ICarryable carryable)
	{
		if ((currentCarried.carryable == carryable) || !currentCarried.value)
			StopCarrying();
	}

	public void RegisterFrameDependentPhysicsInteraction((PlayerPhysicsInteractionType triggerType, Collider2D collider2D, Collision2D collision2D) interaction)
	{
		if (!physicsInteractionQueue.Contains(interaction))
			physicsInteractionQueue.Enqueue(interaction);
	}

	private void LimitVelocity(Vector2 maxVelocity)
	{
		if (maxVelocity.x != 0)
			selfRigidbody.velocityX = Math.Clamp(selfRigidbody.velocityX, -maxVelocity.x, maxVelocity.x);

		if (maxVelocity.y != 0)
			selfRigidbody.velocityY = Math.Clamp(selfRigidbody.velocityY, -maxVelocity.y, maxVelocity.y);
	}

	public void Jump()
	{
		if (State is PlayerStateType.Jumping)
			return;

		selfRigidbody.AddForceY(jumpingForce, ForceMode2D.Impulse);
		State = PlayerStateType.Jumping;
	}

	private bool TryGetInteractable(out IInteractable interactable)
	{
		interactable = default;
		interactablesInRangeList.RemoveAll((iteratedInteractableValue) => !iteratedInteractableValue.value);

		if (interactablesInRangeList.Count > 0)
		{
			interactable = interactablesInRangeList[^1].interactable;
			return true;
		}

		return false;
	}

	public void DoFrameDependentPhysics()
	{
		for (int i = physicsInteractionQueue.Count - 1; i >= 0; i--)
		{
			var iteratedPhysicsInteraction = physicsInteractionQueue.Dequeue();

			switch (iteratedPhysicsInteraction.triggerType)
			{
				case PlayerPhysicsInteractionType.InteractTriggerEnter2D:
				DoInteractTriggerEnter2D(iteratedPhysicsInteraction);
				break;

				case PlayerPhysicsInteractionType.InteractTriggerExit2D:
				DoInteractTriggerExit2D(iteratedPhysicsInteraction);
				break;
			}
		}
	}

	private void DoInteractTriggerEnter2D((PlayerPhysicsInteractionType triggerType, Collider2D collider2D, Collision2D collision2D) interaction)
	{
		if (!interaction.collider2D)
			return;

		if (EventReflectorUtils.TryGetComponentByEventReflector<IInteractable>(interaction.collider2D.gameObject, out IInteractable found))
			interactablesInRangeList.Add(new IInteractableValue<Transform>(found, (found as Component).transform));
	}

	private void DoInteractTriggerExit2D((PlayerPhysicsInteractionType triggerType, Collider2D collider2D, Collision2D collision2D) interaction)
	{
		if (!interaction.collider2D)
		{
			interactablesInRangeList.RemoveAll((iteratedInteractableValue) => !iteratedInteractableValue.value);
			return;
		}

		if (EventReflectorUtils.TryGetComponentByEventReflector<IInteractable>(interaction.collider2D.gameObject, out IInteractable found))
			interactablesInRangeList.Remove(new IInteractableValue<Transform>(found, (found as Component).transform));
	}

	private void DoCarried()
	{
		if (currentCarried.value)
			currentCarried.value.MovePosition(defaultCarryPoint.position);
	}

	protected override void DoIdle()
	{
		if (!IsGrounded())
		{
			State = PlayerStateType.Flying;
			return;
		}

		if (norDirHorizontalInput != 0)
		{
			if (isRunningActivated)
				State = PlayerStateType.Running;
			else
				State = PlayerStateType.Walking;
		}
	}

	protected override void DoWalking()
	{
		if (!IsGrounded())
		{
			State = PlayerStateType.Flying;
			return;
		}

		if (norDirHorizontalInput == 0)
			State = PlayerStateType.Idle;
		else if (isRunningActivated)
			State = PlayerStateType.Running;
	}

	protected override void DoRunning()
	{
		if (!IsGrounded())
		{
			State = PlayerStateType.Flying;
			return;
		}

		if (norDirHorizontalInput == 0)
			State = PlayerStateType.Idle;
		else if (!isRunningActivated)
			State = PlayerStateType.Walking;
	}

	protected override void DoFlying()
	{
		if (IsGrounded())
		{
			State = PlayerStateType.Idle;
			return;
		}
	}

	protected override void DoJumping()
	{
		var isGrounded = IsGrounded();
		var isAbleToSwitchStates = jumpingReleaseStateTimer.Tick() || isGrounded;

		if (jumpingBlockStateTimer.Tick() && isAbleToSwitchStates)
		{
			jumpingBlockStateTimer.Reset();
			jumpingReleaseStateTimer.Reset();

			if (isGrounded)
				State = PlayerStateType.Idle;
			else
				State = PlayerStateType.Flying;
		}
	}

	protected override void DoWalkingFixed()
	{
		selfRigidbody.AddForceX(walkingForce * norDirHorizontalInput, ForceMode2D.Impulse);
		LimitVelocity(walkingMaxVelocity);
	}

	protected override void DoRunningFixed()
	{
		selfRigidbody.AddForceX(runningForce * norDirHorizontalInput, ForceMode2D.Impulse);
		LimitVelocity(runningMaxVelocity);
	}

	protected override void DoJumpingFixed()
	{
		selfRigidbody.AddForceX(walkingForce * norDirHorizontalInput, ForceMode2D.Impulse);
		LimitVelocity(runningMaxVelocity);
	}

	public void OnInteractTriggerEnter2D(Collider2D collider)
		=> RegisterFrameDependentPhysicsInteraction((PlayerPhysicsInteractionType.InteractTriggerEnter2D, collider, null));

	public void OnInteractTriggerExit2D(Collider2D collider)
		=> RegisterFrameDependentPhysicsInteraction((PlayerPhysicsInteractionType.InteractTriggerExit2D, collider, null));

	// TODO: Refactor interaction
	private void OnInputInteractPerformed(CallbackContext context)
	{
		if (TryGetInteractable(out IInteractable selectedInteractable))
		{
			if (selectedInteractable is BabyChickenAI)
				InteractWithBabyChickenAI(selectedInteractable);
		}
	}

	private void InteractWithBabyChickenAI(IInteractable interactableBabyChickenAI)
	{
		// Interact
		var sendValue = new PlayerInteractionArgs
		{
			WantsToCarry = true
		};

		interactableBabyChickenAI.Interact(this, sendValue, out InteractionArgs resultValue);

		// Do action by result
		var convertedResultValue = resultValue as BabyChickenAIInteractionArgs;
		var carryableBabyChickenAI = interactableBabyChickenAI as ICarryable;

		if (convertedResultValue.InteractorAbleToCarrySelf)
		{
			Carry(carryableBabyChickenAI, convertedResultValue.ChickenRigidbody);
			isInteractionBlocked = true;
		}
		else
		{
			StopCarrying(carryableBabyChickenAI);
			isInteractionBlocked = false;
		}
	}

	private void OnInputJumpPerformed(CallbackContext context)
	{
		Jump();
	}

	private void OnInputSprintPerformed(CallbackContext context)
	{
		isRunningActivated = true;
	}

	private void OnInputSprintCanceled(CallbackContext context)
	{
		isRunningActivated = false;
	}

	private void OnInputMovePerformed(CallbackContext ctx)
	{
		norDirHorizontalInput = (sbyte)MathF.Sign(ctx.ReadValue<Vector2>().x);
	}

	private void OnInputMoveCanceled(CallbackContext ctx)
	{
		norDirHorizontalInput = 0;
	}

	public bool IsAbleToJumpTowards(Vector2 worldPosition)
	{
		var distSelfToDestination = (worldPosition - selfRigidbody.position);
		var isInsideAngle = Vector3.Angle(Vector2.up, distSelfToDestination) <= (jumpingAngle * 0.5f);
		return isInsideAngle;
	}


	// Dispose
	private void OnDisable()
	{
		State = PlayerStateType.Idle;
		inputActions.Player.Disable();
	}
}


#if UNITY_EDITOR

public sealed partial class Player
{
	private void OnDrawGizmosSelected()
	{
		DrawRaycastBoundsGizmos();
	}

	private void DrawRaycastBoundsGizmos()
	{
		Gizmos.color = new Color(1f, 0f, 0f, 0.25f);
		Gizmos.DrawCube(this.transform.position, raycastBounds);
	}
}

#endif