using UnityEngine;

public sealed partial class PlayerInteractableBabyFoxAI : InteractableMonoBehaviourBase<BabyFoxAI, Player, PlayerInteractorBabyFoxAI>
{
	[SerializeField]
	private CarryableMonoBehaviourBase<BabyFoxAI, Player> _carryableBabyFoxAI;

	public CarryableMonoBehaviourBase<BabyFoxAI, Player> CarryableBabyFoxAI => _carryableBabyFoxAI;


	// Update
	public override bool Interact(PlayerInteractorBabyFoxAI interactor, InteractionType interactionType)
	{
		switch (interactionType)
		{
			case InteractionType.Carry:
			interactor.CarrierOfBabyFoxAI.Carry(_carryableBabyFoxAI, RelativeTo.SelfRigidbody);
			return true;

			case InteractionType.UnCarry:
			interactor.CarrierOfBabyFoxAI.StopCarrying(_carryableBabyFoxAI, RelativeTo.SelfRigidbody);
			return true;

			case InteractionType.KillTarget:
			RelativeTo.TakeDamage(RelativeTo.MaxHealth, default);
			return true;
		}

		Debug.LogWarningFormat("Interaction {0} didnt programmed", interactionType);
		return false;
	}
}


#if UNITY_EDITOR

public sealed partial class PlayerInteractableBabyFoxAI
{ }

#endif