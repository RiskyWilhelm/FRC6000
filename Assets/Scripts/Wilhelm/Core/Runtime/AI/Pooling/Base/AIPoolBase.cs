using UnityEngine;

public abstract partial class AIPoolBase<SingletonType, AIType> : MonoBehaviourSingletonPoolBase<SingletonType, AIType>
	where SingletonType : AIPoolBase<SingletonType, AIType>
	where AIType : AIBase
{
	[SerializeField]
	[Tooltip("Used for copying when creating a new AI in pool")]
	private AIType prefab;


	// Initialize
	protected override AIType OnCreatePooledObject()
	{
		return Instantiate(prefab);
	}

	protected override void OnGetPooledObject(AIType pooledObject)
	{
		pooledObject.gameObject.SetActive(true);
	}


	// Dispose
	protected override void OnReleasePooledObject(AIType pooledObject)
	{
		// TODO: Set position to random base
		pooledObject.gameObject.SetActive(false);
	}

	protected override void OnDestroyPooledObject(AIType pooledObject)
	{
		Destroy(pooledObject.gameObject);
	}
}


#if UNITY_EDITOR

public abstract partial class AIPoolBase<SingletonType, AIType>
{ }

#endif