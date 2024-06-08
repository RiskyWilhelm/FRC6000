public interface IAITarget
{
	public ushort Health { get; }

	public ushort Power { get; }

	public bool IsDead { get; }

	public void TakeDamage(uint damage);
}