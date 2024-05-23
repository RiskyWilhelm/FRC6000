using UnityEngine;
using UnityEngine.Pool;

public abstract partial class MonoBehaviourSingletonPoolBase<SingletonType, PooledObjectType> : MonoBehaviourSingletonBase<SingletonType>
    where SingletonType : MonoBehaviourSingletonPoolBase<SingletonType, PooledObjectType>
    where PooledObjectType : class
{
    [SerializeField]
    private bool collectionCheck = true;

    [SerializeField]
    private int maxPoolSize;

	protected ObjectPool<PooledObjectType> objectPool;


	// Initialize
	protected override void Awake()
	{
		objectPool = new ObjectPool<PooledObjectType>(OnCreatePooledObject, OnGetPooledObject, OnReleasePooledObject, OnDestroyPooledObject, collectionCheck, 10, maxPoolSize);
		base.Awake();
	}

	protected abstract PooledObjectType OnCreatePooledObject();

	protected abstract void OnGetPooledObject(PooledObjectType pooledObject);


	// Update

	public static PooledObjectType Get() => instance.objectPool.Get();

	public static PooledObject<PooledObjectType> Get(out PooledObjectType pooledObject) => instance.objectPool.Get(out pooledObject);

	public static void Release(PooledObjectType obj) => instance.objectPool.Release(obj);

	public static void Clear() => instance.objectPool.Clear();


	// Dispose
	protected abstract void OnDestroyPooledObject(PooledObjectType pooledObject);

	protected abstract void OnReleasePooledObject(PooledObjectType pooledObject);
}


#if UNITY_EDITOR

public abstract partial class MonoBehaviourSingletonPoolBase<SingletonType, PooledObjectType>
{ }

#endif