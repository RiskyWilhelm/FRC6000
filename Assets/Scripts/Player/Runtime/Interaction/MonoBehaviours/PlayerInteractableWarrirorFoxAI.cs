using UnityEngine;

public sealed partial class PlayerInteractableWarrirorFoxAI : InteractableMonoBehaviourBase<WarrirorFoxAI, Player, PlayerInteractorBabyFoxAI>
{
	// Update
	public override bool Interact(PlayerInteractorBabyFoxAI interactor, InteractionType interactionType)
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

public sealed partial class PlayerInteractableWarrirorFoxAI
{ }

#endif
