using System;
using System.Collections.Generic;
using UnityEngine;

public sealed partial class BabyFoxAI : GroundedAIBase, IHomeAccesser, IInteractable, ICarryable, IFrameDependentPhysicsInteractor<BabyFoxAIPhysicsInteractionType>
{
	[Header("BabyFoxAI Movement")]
	#region BabyFoxAI Movement

	[SerializeField]
	private TimerRandomized goHomeBackTimer = new(10f, 10f, 60f);


	#endregion

	#region BabyFoxAI Carry

	[field: NonSerialized]
	public ICarrier Carrier { get; private set; }


	#endregion

	#region BabyFoxAI Other

	[field: NonSerialized]
	public bool OpenAIHomeGate { get; private set; }

	[field: NonSerialized]
	public HomeBase ParentHome { get; set; }

	[NonSerialized]
	private readonly Queue<(BabyFoxAIPhysicsInteractionType triggerType, Collider2D collider2D, Collision2D collision2D)> physicsInteractionQueue = new();


	#endregion


	// Initialize
	protected override void OnEnable()
	{
		PlayerControllerSingleton.Instance.onTargetBirthEventDict[TargetType.BabyFox]?.Invoke();
		goHomeBackTimer.ResetAndRandomize();

		base.OnEnable();
	}


	// Update
	protected override void Update()
	{
		goHomeBackTimer.Tick();
		DoFrameDependentPhysics();
		base.Update();
	}

	public void RegisterFrameDependentPhysicsInteraction((BabyFoxAIPhysicsInteractionType triggerType, Collider2D collider2D, Collision2D collision2D) interaction)
	{
		if (!physicsInteractionQueue.Contains(interaction))
			physicsInteractionQueue.Enqueue(interaction);
	}

	public void Interact(IInteractor interactor, InteractionArgs receivedValue, out InteractionArgs resultValue)
	{
		resultValue = InteractionArgs.Empty;

		if (interactor is Player)
			OnInteractedByPlayer(interactor, receivedValue, out resultValue);
	}

	public bool TrySetDestinationToHome()
	{
		if (ParentHome != null)
			return OpenAIHomeGate = this.TrySetDestinationToTransform(ParentHome.transform, 0.1f);

		return false;
	}

	public void DoFrameDependentPhysics()
	{
		for (int i = physicsInteractionQueue.Count - 1; i >= 0; i--)
		{
			var iteratedPhysicsInteraction = physicsInteractionQueue.Dequeue();

			switch (iteratedPhysicsInteraction.triggerType)
			{
				case BabyFoxAIPhysicsInteractionType.EnemyTriggerStay2D:
				DoEnemyTriggerStay2D(iteratedPhysicsInteraction);
				break;
			}
		}
	}

	private void DoEnemyTriggerStay2D((BabyFoxAIPhysicsInteractionType triggerType, Collider2D collider2D, Collision2D collision2D) interaction)
	{
		if (!interaction.collider2D)
			return;

		if (State is PlayerStateType.Blocked)
			return;

		if (EventReflectorUtils.TryGetComponentByEventReflector<ITarget>(interaction.collider2D.gameObject, out ITarget foundTarget))
		{
			// Try Runaway from target
			if (!runawayTargetTypeList.Contains(foundTarget.TargetTag))
				return;

			if (TrySetDestinationAwayFromVector((foundTarget as Component).transform.position))
			{
				State = PlayerStateType.Running;
				OpenAIHomeGate = true;
			}
		}
	}

	protected override void DoIdle()
	{
		// If not grounded, set state to Flying
		if (!IsGrounded())
		{
			State = PlayerStateType.Flying;
			return;
		}

		// If wants to go home, set state to walking
		if (goHomeBackTimer.HasEnded)
		{
			goHomeBackTimer.ResetAndRandomize();

			if (TrySetDestinationToHome())
			{
				State = PlayerStateType.Walking;
				return;
			}
		}

		// Otherwise, continue old idle
		base.DoIdle();
	}

	protected override void OnStateChangedToDead()
	{
		PlayerControllerSingleton.Instance.onTargetDeathEventDict[TargetType.BabyFox]?.Invoke();
		ReleaseOrDestroySelf();
		base.OnStateChangedToDead();
	}

	protected override void OnChangedDestination(Vector2? newDestination)
	{
		OpenAIHomeGate = false;
		base.OnChangedDestination(newDestination);
	}

	public void OnEnemyTriggerStay2D(Collider2D collider)
		=> RegisterFrameDependentPhysicsInteraction((BabyFoxAIPhysicsInteractionType.EnemyTriggerStay2D, collider, null));

	private void OnInteractedByPlayer(IInteractor interactor, InteractionArgs receivedValue, out InteractionArgs resultValue)
	{
		var interactorArgs = receivedValue as PlayerInteractionArgs;
		var convertedResultValue = new BabyFoxAIInteractionArgs
		{
			FoxRigidbody = selfRigidbody
		};

		if (State is PlayerStateType.Blocked)
			convertedResultValue.InteractorAbleToCarrySelf = false;
		else if (interactorArgs.WantsToCarry)
			convertedResultValue.InteractorAbleToCarrySelf = true;

		resultValue = convertedResultValue;
	}

	public void OnCarried(ICarrier carrier)
	{
		Carrier = carrier;
		State = PlayerStateType.Blocked;
	}

	public void OnUncarried(ICarrier carrier)
	{
		Carrier = null;
		State = PlayerStateType.Idle;
	}

	public void OnEnteredAIHome(HomeBase home)
	{
		ReleaseOrDestroySelf();
	}

	public void OnLeftFromAIHome(HomeBase home)
	{
		OpenAIHomeGate = false;
	}

	public override void CopyTo(in AIBase main)
	{
		if (main is BabyFoxAI foundSelf)
			foundSelf.goHomeBackTimer = this.goHomeBackTimer;

		base.CopyTo(main);
	}


	// Dispose
	protected override void OnDisable()
	{
		if (GameControllerSingleton.IsQuitting)
			return;

		Carrier?.StopCarrying(this);
		DoFrameDependentPhysics();
		base.OnDisable();
	}
}


#if UNITY_EDITOR

public sealed partial class BabyFoxAI
{ }

#endif