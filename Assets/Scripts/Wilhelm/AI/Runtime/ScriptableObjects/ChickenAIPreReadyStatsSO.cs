using UnityEngine;

[CreateAssetMenu]
public sealed partial class ChickenAIPreReadyStatsSO : ScriptableObject, ICopyable<ChickenAIStats>
{
	[SerializeField]
	private ChickenAIStats preReadyStats;

	public void CopyTo(in ChickenAIStats main) => preReadyStats.CopyTo(main);
}


#if UNITY_EDITOR

public sealed partial class ChickenAIPreReadyStatsSO
{ }

#endif