using UnityEngine;

public sealed partial class PlayerInteractableWarrirorChickenAI : InteractableMonoBehaviourBase<WarrirorChickenAI, Player, PlayerInteractorWarrirorChickenAI>
{
	// Update
	public override bool Interact(PlayerInteractorWarrirorChickenAI interactor, InteractionType interactionType)
	{
		switch (interactionType)
		{
			case InteractionType.GoHome:
			RelativeTo.ForceToGoHome();
			interactor.RelativeTo.TakeDamage(2, default);
			return true;
		}

		Debug.LogWarningFormat("Interaction {0} didnt programmed", interactionType);
		return false;
	}
}


#if UNITY_EDITOR

public sealed partial class PlayerInteractableWarrirorChickenAI
{ }

#endif