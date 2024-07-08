using UnityEngine;

public class BabyFoxAIInteractionArgs : InteractionArgs
{
	public Rigidbody2D FoxRigidbody { get; set; }

	public bool InteractorAbleToCarrySelf { get; set; }


	public override void Dispose()
	{
		FoxRigidbody = default;
		InteractorAbleToCarrySelf = default;
	}
}