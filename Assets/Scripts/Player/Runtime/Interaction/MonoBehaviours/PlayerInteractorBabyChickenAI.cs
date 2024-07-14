using UnityEngine;

public sealed partial class PlayerInteractorBabyChickenAI : InteractorMonoBehaviourBase<Player>
{
	[SerializeField]
	private CarrierMonoBehaviourBase<Player, BabyChickenAI> _carrierOfBabyChickenAI;

	public CarrierMonoBehaviourBase<Player, BabyChickenAI> CarrierOfBabyChickenAI => _carrierOfBabyChickenAI;
}


#if UNITY_EDITOR

public sealed partial class PlayerInteractorBabyChickenAI
{ }

#endif