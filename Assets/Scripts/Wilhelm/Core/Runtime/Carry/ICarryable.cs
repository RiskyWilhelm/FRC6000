public interface ICarryable
{
	public ICarrier Carrier { get; }


	public void OnCarried(ICarrier carrier);

	public void OnUncarried(ICarrier carrier);
}