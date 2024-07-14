using UnityEngine;

public sealed partial class PlayerInteractorBabyFoxAI : InteractorMonoBehaviourBase<Player>
{
	[SerializeField]
	private CarrierMonoBehaviourBase<Player, BabyFoxAI> _carrierOfBabyFoxAI;

	public CarrierMonoBehaviourBase<Player, BabyFoxAI> CarrierOfBabyFoxAI => _carrierOfBabyFoxAI;
}


#if UNITY_EDITOR

public sealed partial class PlayerInteractorBabyFoxAI
{ }

#endif