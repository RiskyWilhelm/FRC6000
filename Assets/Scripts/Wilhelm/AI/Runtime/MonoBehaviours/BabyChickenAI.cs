using System;
using System.Collections.Generic;
using UnityEngine;

public sealed partial class BabyChickenAI : GroundedAIBase, IHomeAccesser, IFrameDependentPhysicsInteractor<BabyChickenAIPhysicsInteractionType>
{
	[Header("BabyChickenAI Movement")]
	#region BabyChickenAI Movement

	[SerializeField]
	private TimerRandomized goHomeBackTimer = new(10f, 10f, 60f);


	#endregion

	#region BabyChickenAI Other

	[field: NonSerialized]
	public bool OpenAIHomeGate { get; private set; }

	[field: NonSerialized]
	public HomeBase ParentHome { get; set; }

	[NonSerialized]
	private readonly Queue<(BabyChickenAIPhysicsInteractionType triggerType, Collider2D collider2D, Collision2D collision2D)> physicsInteractionQueue = new();


	#endregion


	// Initialize
	protected override void OnEnable()
	{
		GameControllerSingleton.Instance.onTargetBirthDict[TargetType.BabyChicken]?.Invoke();
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

	public void RegisterFrameDependentPhysicsInteraction((BabyChickenAIPhysicsInteractionType triggerType, Collider2D collider2D, Collision2D collision2D) interaction)
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
				case BabyChickenAIPhysicsInteractionType.EnemyTriggerStay2D:
				DoEnemyTriggerStay2D(iteratedPhysicsInteraction);
				break;
			}
		}
	}

	private void DoEnemyTriggerStay2D((BabyChickenAIPhysicsInteractionType triggerType, Collider2D collider2D, Collision2D collision2D) interaction)
	{
		if (!interaction.collider2D)
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
			goHomeBackTimer.ResetAndRandomize();
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
		GameControllerSingleton.Instance.onTargetDeathDict[TargetType.BabyChicken]?.Invoke();
		ReleaseOrDestroySelf();
		base.OnStateChangedToDead();
	}

	protected override void OnChangedDestination(Vector2? newDestination)
	{
		OpenAIHomeGate = false;
		base.OnChangedDestination(newDestination);
	}

	public void OnEnemyTriggerStay2D(Collider2D collider)
		=> RegisterFrameDependentPhysicsInteraction((BabyChickenAIPhysicsInteractionType.EnemyTriggerStay2D, collider, null));

	public void OnEnteredAIHome(HomeBase home)
	{
		ReleaseOrDestroySelf();
	}

	public void OnLeftFromAIHome(HomeBase home)
	{
		OpenAIHomeGate = false;
	}

	public void OnStolenByFoxHome(FoxAIHome foxAIHome)
	{
		GameControllerSingleton.Instance.onTargetDeathDict[TargetType.BabyChicken]?.Invoke();
		ReleaseOrDestroySelf();
	}

	public override void CopyTo(in AIBase main)
	{
		if (main is BabyChickenAI foundSelf)
			foundSelf.goHomeBackTimer = this.goHomeBackTimer;

		base.CopyTo(main);
	}


	// Dispose
	protected override void OnDisable()
	{
		if (GameControllerSingleton.IsQuitting)
			return;

		DoFrameDependentPhysics();
		base.OnDisable();
	}
}


#if UNITY_EDITOR

public sealed partial class BabyChickenAI
{ }

#endif