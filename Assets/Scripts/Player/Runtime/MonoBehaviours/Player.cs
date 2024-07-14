using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Pool;
using static UnityEngine.InputSystem.InputAction;

[RequireComponent(typeof(Rigidbody2D))]
[DisallowMultipleComponent]
public sealed partial class Player : StateMachineDrivenPlayerBase, ITarget, IFrameDependentPhysicsInteractor<PlayerPhysicsInteractionType>
{
	[field: Header("Player Stats")]
	#region Player Stats

	[SerializeField]
	private uint _health;

	public uint Health
	{
		get => _health;
		private set
		{
			if (value != _health)
			{
				_health = value;
				onHealthChanged?.Invoke(value);
			}
		}
	}

	[field: SerializeField]
	public uint MaxHealth { get; private set; }

	[SerializeField]
	private Timer regenerateHealthTimer;


	#endregion

	[field: Header("Player Target")]
	#region Player Target

	[field: SerializeField]
	public TargetType TargetTag { get; private set; }

	public List<TargetType> acceptedTargetTypeList = new();


	#endregion

	[Header("Player Single Normal Attack")]
	#region Player Single Normal Attack

	[SerializeField]
	private uint singleNormalAttackDamage = 1;

	[SerializeField]
	[Tooltip("Sticks the state")]
	private Timer singleNormalAttackBlockTimer;

	[NonSerialized]
	private (ITarget target, Coroutine coroutine) currentSingleNormalAttack;

	[NonSerialized]
	private readonly HashSet<ITargetValue<Transform>> singleNormalAttackTargetInRangeSet = new();

	public bool IsSingleNormalAttacking => (currentSingleNormalAttack != default);


	#endregion

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

	[Header("Player Visuals")]
	#region Player Visuals

	[SerializeField]
	private Animator animator;


	#endregion

	[Header("AIBase Events")]
	#region AIBase Events

	[SerializeField]
	private UnityEvent<uint> onHealthChanged = new();


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
	protected override void OnEnable()
	{
		inputActions?.Player.Enable();
		base.OnEnable();
	}

	private void Start()
	{
		inputActions = new FRC_Default_InputActions();
		inputActions.Player.Move.performed += OnInputMovePerformed;
		inputActions.Player.Move.canceled += OnInputMoveCanceled;

		inputActions.Player.Sprint.performed += OnInputSprintPerformed;
		inputActions.Player.Sprint.canceled += OnInputSprintCanceled;

		inputActions.Player.Jump.performed += OnInputJumpPerformed;
		inputActions.Player.Attack.performed += OnInputAttackPerformed;
		inputActions.Player.Enable();

		GameControllerPersistentSingleton.Instance.onLostGame.AddListener(DisablePlayerInput);
	}


	// Update
	protected override void Update()
	{
		DoFrameDependentPhysics();
		base.Update();
	}

	public void RegisterFrameDependentPhysicsInteraction((PlayerPhysicsInteractionType triggerType, Collider2D collider2D, Collision2D collision2D) interaction)
	{
		if (!physicsInteractionQueue.Contains(interaction))
			physicsInteractionQueue.Enqueue(interaction);
	}

	public void EnablePlayerInput()
	{
		inputActions.Player.Enable();
	}

	public void DisablePlayerInput()
	{
		inputActions.Player.Disable();
	}

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

	private void LimitVelocity(Vector2 maxVelocity)
	{
		if (maxVelocity.x != 0)
			SelfRigidbody.velocityX = Math.Clamp(SelfRigidbody.velocityX, -maxVelocity.x, maxVelocity.x);

		if (maxVelocity.y != 0)
			SelfRigidbody.velocityY = Math.Clamp(SelfRigidbody.velocityY, -maxVelocity.y, maxVelocity.y);
	}

	public void Jump()
	{
		if (State is PlayerStateType.Jumping)
			return;

		SelfRigidbody.AddForceY(jumpingForce, ForceMode2D.Impulse);
		State = PlayerStateType.Jumping;
	}

	private bool TryDoSingleNormalAttackTo(ITarget target)
	{
		// Prevents StartCoroutine to get called when self is disabled
		if (!this.gameObject.activeSelf)
			return false;

		// If attacking or target dead, let it finish first
		if (IsSingleNormalAttacking || target.IsDead)
			return false;

		if (State is PlayerStateType.Jumping or PlayerStateType.Flying or PlayerStateType.Attacking or PlayerStateType.Blocked or PlayerStateType.Dead)
			return false;

		// If target is not accepted, return
		if (!acceptedTargetTypeList.Contains(target.TargetTag))
			return false;

		// Do the attack
		currentSingleNormalAttack.target = target;
		currentSingleNormalAttack.coroutine = StartCoroutine(DoSingleNormalAttack(target));
		return true;
	}

	private bool TryCancelSingleNormalAttackFrom(ITarget target)
	{
		if (target == currentSingleNormalAttack.target)
		{
			RefreshSingleNormalAttack();
			return true;
		}

		return false;
	}

	private void RefreshSingleNormalAttack()
	{
		if (currentSingleNormalAttack.coroutine != null)
			StopCoroutine(currentSingleNormalAttack.coroutine);

		if (State == PlayerStateType.Attacking)
			State = PlayerStateType.Idle;

		currentSingleNormalAttack = default;
		singleNormalAttackBlockTimer.Reset();
	}

	private void RefreshAttackState()
	{
		RefreshSingleNormalAttack();
	}

	private IEnumerator DoSingleNormalAttack(ITarget target)
	{
		// Take full control over the body by setting state to attacking, ready timer
		State = PlayerStateType.Attacking;

		// Lock the state to Attacking until timer ends
		while (!singleNormalAttackBlockTimer.Tick())
			yield return null;

		// Target is killed by unknown thing while attacking
		if (target.IsDead)
		{
			RefreshSingleNormalAttack();
			yield break;
		}

		// Do the attack
		target.TakeDamage(singleNormalAttackDamage, SelfRigidbody.position);

		if (target.IsDead)
			OnKilledTarget(target);

		RefreshSingleNormalAttack();
	}

	public void DoFrameDependentPhysics()
	{
		for (int i = physicsInteractionQueue.Count - 1; i >= 0; i--)
		{
			var iteratedPhysicsInteraction = physicsInteractionQueue.Dequeue();

			switch (iteratedPhysicsInteraction.triggerType)
			{
				case PlayerPhysicsInteractionType.EnemyTriggerEnter2D:
				DoSingleNormalAttackTriggerEnter2D(iteratedPhysicsInteraction);
				break;

				case PlayerPhysicsInteractionType.EnemyTriggerExit2D:
				DoSingleNormalAttackTriggerExit2D(iteratedPhysicsInteraction);
				break;
			}
		}
	}

	private void DoSingleNormalAttackTriggerEnter2D((PlayerPhysicsInteractionType triggerType, Collider2D collider2D, Collision2D collision2D) interaction)
	{
		if (!interaction.collider2D)
			return;

		if (EventReflectorUtils.TryGetComponentByEventReflector<ITarget>(interaction.collider2D.gameObject, out ITarget foundTarget))
			singleNormalAttackTargetInRangeSet.Add(new ITargetValue<Transform>(foundTarget, (foundTarget as Component).transform));
	}

	private void DoSingleNormalAttackTriggerExit2D((PlayerPhysicsInteractionType triggerType, Collider2D collider2D, Collision2D collision2D) interaction)
	{
		if (!interaction.collider2D)
		{
			singleNormalAttackTargetInRangeSet.RemoveWhere((iteratedTargetTuple) => !iteratedTargetTuple.value);
			return;
		}

		if (EventReflectorUtils.TryGetComponentByEventReflector<ITarget>(interaction.collider2D.gameObject, out ITarget foundTarget))
			singleNormalAttackTargetInRangeSet.Remove(new ITargetValue<Transform>(foundTarget, (foundTarget as Component).transform));
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

			regenerateHealthTimer.Reset();
		}
		else if (regenerateHealthTimer.Tick())
		{
			animator.Play("Sleeping");
			Health = Math.Clamp(checked(Health + 1), 0, MaxHealth);
			regenerateHealthTimer.Reset();
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
		SelfRigidbody.AddForceX(walkingForce * norDirHorizontalInput, ForceMode2D.Impulse);
		LimitVelocity(walkingMaxVelocity);
	}

	protected override void DoRunningFixed()
	{
		SelfRigidbody.AddForceX(runningForce * norDirHorizontalInput, ForceMode2D.Impulse);
		LimitVelocity(runningMaxVelocity);
	}

	protected override void DoJumpingFixed()
		=> DoRunningFixed();

	protected override void OnStateChangedToIdle()
	{
		animator.Play("Idle");
	}

	protected override void OnStateChangedToWalking()
	{
		animator.Play("Walking");
	}

	protected override void OnStateChangedToRunning()
	{
		animator.Play("Running");
	}

	protected override void OnStateChangedToJumping()
	{
		animator.Play("Jumping");
	}

	protected override void OnStateChangedToAttacking()
	{
		animator.Play("Attacking");
	}

	protected override void OnStateChangedToDead()
	{
		GameControllerPersistentSingleton.Instance.LostGame();
	}

	protected override void OnStateChangedToAny(PlayerStateType newState)
	{
		if (newState is not PlayerStateType.Attacking)
			RefreshAttackState();
	}

	private void OnInputJumpPerformed(CallbackContext context)
	{
		Jump();
	}

	private void OnInputAttackPerformed(CallbackContext context)
	{
		var cachedAcceptedTargetDict = DictionaryPool<Transform, ITarget>.Get();

		// Select valid targets in range
		foreach (var iteratedTargetValue in singleNormalAttackTargetInRangeSet)
		{
			if (acceptedTargetTypeList.Contains(iteratedTargetValue.target.TargetTag))
				cachedAcceptedTargetDict.Add(iteratedTargetValue.value, iteratedTargetValue.target);
		}

		// If there is any runaway target in range, runaway from it and discard other targets
		if (cachedAcceptedTargetDict.Count > 0)
		{
			this.transform.TryGetNearestTransform(cachedAcceptedTargetDict.Keys.GetEnumerator(), out Transform nearestTransform);
			TryDoSingleNormalAttackTo(cachedAcceptedTargetDict[nearestTransform]);
		}
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

	// TODO: Such a good method to use for dislaying something in UI
	private void OnKilledTarget(ITarget target)
	{ }

	public void OnSingleNormalAttackTriggerEnter2D(Collider2D collider)
		=> RegisterFrameDependentPhysicsInteraction((PlayerPhysicsInteractionType.EnemyTriggerEnter2D, collider, null));

	public void OnSingleNormalAttackTriggerExit2D(Collider2D collider)
		=> RegisterFrameDependentPhysicsInteraction((PlayerPhysicsInteractionType.EnemyTriggerExit2D, collider, null));

	public bool IsAbleToJumpTowards(Vector2 worldPosition)
	{
		var distSelfToDestination = (worldPosition - SelfRigidbody.position);
		var isInsideAngle = Vector3.Angle(Vector2.up, distSelfToDestination) <= (jumpingAngle * 0.5f);
		return isInsideAngle;
	}


	// Dispose
	private void OnDestroy()
	{
		GameControllerPersistentSingleton.Instance.onLostGame.RemoveListener(DisablePlayerInput);
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