public interface IAITarget
{
	public byte Power { get; }


	/// <summary> Called when chaser AI attacked to self </summary>
	public void OnGotAttacked(AIBase chaser);
}