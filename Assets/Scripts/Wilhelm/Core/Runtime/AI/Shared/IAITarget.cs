public interface IAITarget
{
	public ushort Power { get; }

	public ushort Health { get; }


	/// <summary> Called when chaser AI attacked to self </summary>
	public void OnGotAttackedBy(AIBase chaser);
}