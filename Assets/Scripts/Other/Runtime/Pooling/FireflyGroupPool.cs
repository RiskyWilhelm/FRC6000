using UnityEngine;

public sealed partial class FireflyGroupPool : MonoBehaviourPoolBase<FireflyGroup>
{
	[Header("FireflyGroupPool Spawn")]
	#region

	[SerializeField]
	[Tooltip("Used for creating")]
	private FireflyGroup prefab;

	#endregion


	// Initialize
	protected override FireflyGroup OnCreatePooledObject()
	{
		var craeted = Instantiate(prefab);
		craeted.ParentPool = this;

		return craeted;
	}

	protected override void OnGetPooledObject(FireflyGroup pooledObject)
	{
		pooledObject.enabled = true;
		base.OnGetPooledObject(pooledObject);
	}


	// Dispose
	protected override void OnReleasePooledObject(FireflyGroup pooledObject)
	{
		pooledObject.enabled = false;
		base.OnReleasePooledObject(pooledObject);
	}

	protected override void OnDestroyPooledObject(FireflyGroup pooledObject)
	{
		Destroy(pooledObject.gameObject);
	}
}


#if UNITY_EDITOR

public sealed partial class FireflyGroupPool
{ }

#endif