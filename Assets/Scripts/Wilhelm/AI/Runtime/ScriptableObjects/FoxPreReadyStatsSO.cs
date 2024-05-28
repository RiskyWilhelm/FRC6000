using UnityEngine;

[CreateAssetMenu]
public sealed partial class FoxAIPreReadyStatsSO : ScriptableObject, ICopyable<FoxAIStats>
{
	[SerializeField]
	private FoxAIStats preReadyStats;

	public void CopyTo(in FoxAIStats main) => preReadyStats.CopyTo(main);
}


#if UNITY_EDITOR

public sealed partial class FoxAIPreReadyStatsSO
{ }

#endif