using FMOD.Studio;
using FMODUnity;
using System;
using System.Collections.Generic;
using UnityEngine;

public sealed partial class BabyFoxAI : GroundedAIBase, IHomeAccesser, IFrameDependentPhysicsInteractor<BabyFoxAIPhysicsInteractionType>
{
	[Header("BabyFoxAI Movement")]
	#region BabyFoxAI Movement

	[SerializeField]
	private TimerRandomized goHomeBackTimer = new(10f, 10f, 60f);


	#endregion

	[Header("BabyFoxAI Visuals")]
	#region BabyFoxAI Visuals

	[SerializeField]
	private Animator animator;


	#endregion

	[Header("BabyFoxAI Sounds")]
	#region BabyFoxAI Sounds

	[SerializeField]
	private EventReference walkingSoundReference;

	[SerializeField]
	private EventReference runningSoundReference;

	[SerializeField]
	private EventReference jumpSoundReference;

	[NonSerialized]
	private EventInstance walkingSoundInstance;

	[NonSerialized]
	private EventInstance runningSoundInstance;


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
	private void Start()
	{
		walkingSoundInstance = RuntimeManager.CreateInstance(walkingSoundReference);
		runningSoundInstance = RuntimeManager.CreateInstance(runningSoundReference);

		RuntimeManager.AttachInstanceToGameObject(walkingSoundInstance, this.transform);
		RuntimeManager.AttachInstanceToGameObject(runningSoundInstance, this.transform);
	}

	protected override void OnEnable()
	{
		PlayerControllerSingleton.onTargetBirthEventDict[TargetType.BabyFox]?.Invoke();
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

			if (TrySetDestinationAwayFromVector((foundTarget as Component).transform.position, isGroundedOnly: true))
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

	protected override void OnStateChangedToIdle()
	{
		animator.Play("Idle");
	}

	protected override void OnStateChangedToWalking()
	{
		RuntimeManager.AttachInstanceToGameObject(walkingSoundInstance, this.transform);
		walkingSoundInstance.start();
		animator.Play("Walking");
	}

	protected override void OnStateChangedToRunning()
	{
		RuntimeManager.AttachInstanceToGameObject(runningSoundInstance, this.transform);
		runningSoundInstance.start();
		animator.Play("Running");
	}

	protected override void OnStateChangedToJumping()
	{
		RuntimeManager.PlayOneShot(jumpSoundReference, this.transform.position);
		animator.Play("Jumping");
	}

	protected override void OnStateChangedToDead()
	{
		PlayerControllerSingleton.onTargetDeathEventDict[TargetType.BabyFox]?.Invoke();
		ReleaseOrDestroySelf();
		base.OnStateChangedToDead();
	}

	protected override void OnStateChangedToBlocked()
	{
		animator.Play("Sleeping");
	}

	protected override void OnStateChangedToAny(PlayerStateType newState)
	{
		walkingSoundInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
		runningSoundInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);

		base.OnStateChangedToAny(newState);
	}

	protected override void OnChangedDestination(Vector2? newDestination)
	{
		OpenAIHomeGate = false;
		base.OnChangedDestination(newDestination);
	}

	public void OnEnemyTriggerStay2D(Collider2D collider)
		=> RegisterFrameDependentPhysicsInteraction((BabyFoxAIPhysicsInteractionType.EnemyTriggerStay2D, collider, null));

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
		walkingSoundInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
		runningSoundInstance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);

		DoFrameDependentPhysics();
		base.OnDisable();
	}

	private void OnDestroy()
	{
		walkingSoundInstance.release();
		runningSoundInstance.release();
	}
}


#if UNITY_EDITOR

public sealed partial class BabyFoxAI
{ }

#endif