using UnityEngine;

public class BabyChickenAIInteractionArgs : InteractionArgs
{
	public Rigidbody2D ChickenRigidbody { get; set; }

	public bool InteractorAbleToCarrySelf { get; set; }


	public override void Dispose()
	{
		ChickenRigidbody = default;
		InteractorAbleToCarrySelf = default;
	}
}