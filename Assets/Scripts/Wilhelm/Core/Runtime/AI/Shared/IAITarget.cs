public interface IAITarget
{
	public ushort Health { get; }

	public bool IsDead { get; }

	public void TakeDamage(uint damage);
}