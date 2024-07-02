using System.Collections;

public static class IEnumeratorExtensions
{
	/// <summary> Allows you to iterate through an IEnumerator </summary>
	public static T GetEnumerator<T>(this T enumerator)
		where T : IEnumerator
	{
		return enumerator;
	}
}