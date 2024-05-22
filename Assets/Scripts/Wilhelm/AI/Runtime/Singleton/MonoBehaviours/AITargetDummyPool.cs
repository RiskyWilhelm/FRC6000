using UnityEngine;

public sealed partial class AITargetDummyPool : MonoBehaviourSingletonPoolBase<AITargetDummyPool, AITargetDummy>
{
	// Initialize
	protected override AITargetDummy OnCreatePooledObject()
	{
		var instantiatedGameObject = new GameObject(nameof(AITargetDummy),
			typeof(AITargetDummy), typeof(CircleCollider2D));

		instantiatedGameObject.layer = Layers.Dynamic;
		instantiatedGameObject.tag = Tags.AITargetDummy;

		// Setup for self-destroy when caught
		var collider = instantiatedGameObject.GetComponent<CircleCollider2D>();
		collider.radius = 0.05f;
		collider.isTrigger = true;

		return instantiatedGameObject.GetComponent<AITargetDummy>();
	}

	protected override void OnGetPooledObject(AITargetDummy pooledObject)
	{
		pooledObject.gameObject.SetActive(true);
	}


	// Update
	public static AITargetDummy Get(Vector3 position)
	{
		var dummy = Get();
		dummy.transform.position = position;
		return dummy;
	}


	// Dispose
	protected override void OnDestroyPooledObject(AITargetDummy pooledObject)
	{
		Destroy(pooledObject.gameObject);
	}

	protected override void OnReleasePooledObject(AITargetDummy pooledObject)
	{
		pooledObject.gameObject.SetActive(false);
	}
}


#if UNITY_EDITOR

public sealed partial class AITargetDummyPool
{ }

#endif