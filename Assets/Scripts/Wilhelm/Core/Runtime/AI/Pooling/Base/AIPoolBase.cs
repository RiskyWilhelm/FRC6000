using UnityEngine;

public abstract partial class AIPoolBase : MonoBehaviourPoolBase<AIBase>
{
	[SerializeField]
	[Tooltip("Used for copying when creating a new AI in pool")]
	private AIBase prefab;


	// Initialize
	protected override AIBase OnCreatePooledObject()
	{
		var newAI = Instantiate(prefab);
		prefab.CopyTo(newAI);

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

public abstract partial class AIPoolBase
{ }

#endif