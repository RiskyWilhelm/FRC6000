using UnityEngine;

public partial class AIPool : MonoBehaviourPoolBase<AIBase>
{
	[SerializeField]
	[Tooltip("If preStats null, this will be used. Used for copying when creating a new AI in pool")]
	private AIBase prefab;


	// Initialize
	protected override AIBase OnCreatePooledObject()
	{
		var newAI = Instantiate(prefab);
		newAI.ParentPool = this;

		return newAI;
	}

	protected override void OnGetPooledObject(AIBase pooledObject)
	{
		pooledObject.Copy(prefab);
		pooledObject.gameObject.SetActive(true);
	}


	// Dispose
	protected override void OnReleasePooledObject(AIBase pooledObject)
	{
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