public interface ICopyable<T>
{
	public void Copy(in T other);
	public void CopyTo(in T main);
}