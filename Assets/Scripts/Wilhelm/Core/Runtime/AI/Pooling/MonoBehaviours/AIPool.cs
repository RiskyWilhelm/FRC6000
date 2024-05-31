using UnityEngine;

public partial class AIPool : MonoBehaviourPoolBase<AIBase>
{
	[SerializeField]
	[Tooltip("Used for copying when creating a new AI in pool")]
	private AIBase prefab;

	[SerializeReference]
	[Tooltip("If null, the prefab will be used. Used for copying the pre stats when creating a new AI in pool")]
	private AIPreReadyStatsSOBase preStats;


	// Initialize
	protected override AIBase OnCreatePooledObject()
	{
		var newAI = Instantiate(prefab);

		if (preStats)
			newAI.Copy(preStats);

		return newAI;
	}

	protected override void OnGetPooledObject(AIBase pooledObject)
	{
		pooledObject.gameObject.SetActive(true);
	}


	// Dispose
	protected override void OnReleasePooledObject(AIBase pooledObject)
	{
		// TODO: Set position to random base
		pooledObject.gameObject.SetActive(false);
	}

	protected override void OnDestroyPooledObject(AIBase pooledObject)
	{
		Destroy(pooledObject.gameObject);
	}
}


#if UNITY_EDITOR

public partial class AIPool
{ }

#endif