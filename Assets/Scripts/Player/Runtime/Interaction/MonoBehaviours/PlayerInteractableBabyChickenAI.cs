using UnityEngine;

public sealed partial class PlayerInteractableBabyChickenAI : InteractableMonoBehaviourBase<BabyChickenAI, Player, PlayerInteractorBabyChickenAI>
{
	[SerializeField]
	private CarryableMonoBehaviourBase<BabyChickenAI, Player> _carryableBabyChickenAI;

	public CarryableMonoBehaviourBase<BabyChickenAI, Player> CarryableBabyChickenAI => _carryableBabyChickenAI;


	// Update
	public override bool Interact(PlayerInteractorBabyChickenAI interactor, InteractionType interactionType)
	{
		switch (interactionType)
		{
			case InteractionType.Carry:
			interactor.CarrierOfBabyChickenAI.Carry(_carryableBabyChickenAI, RelativeTo.SelfRigidbody);
			return true;

			case InteractionType.UnCarry:
			interactor.CarrierOfBabyChickenAI.StopCarrying(_carryableBabyChickenAI, RelativeTo.SelfRigidbody);
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

public sealed partial class PlayerInteractableBabyChickenAI
{ }

#endif