public class PlayerInteractionArgs : InteractionArgs
{
	public bool WantsToCarry {  get; set; }


	public override void Dispose()
	{
		WantsToCarry = default;
	}
}