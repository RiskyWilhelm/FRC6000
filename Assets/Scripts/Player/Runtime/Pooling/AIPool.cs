using UnityEngine;

public partial class AIPool : MonoBehaviourPoolBase<AIBase>
{
	[Header("AIPool Spawn")]
	#region

	[SerializeField]
	[Tooltip("Used for creating")]
	private AIBase prefab;

	#endregion


	// Initialize
	protected override AIBase OnCreatePooledObject()
	{
		var newAI = Instantiate(prefab);
		newAI.ParentPool = this;

		return newAI;
	}

	protected override void OnGetPooledObject(AIBase pooledObject)
	{
		pooledObject.gameObject.SetActive(true);
		base.OnGetPooledObject(pooledObject);
	}


	// Dispose
	protected override void OnReleasePooledObject(AIBase pooledObject)
	{
		pooledObject.gameObject.SetActive(false);
		base.OnReleasePooledObject(pooledObject);
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