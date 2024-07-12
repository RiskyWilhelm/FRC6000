using UnityEngine;
using UnityEngine.Pool;

public abstract partial class MonoBehaviourPoolBase<PooledObjectType> : MonoBehaviour, IPool<PooledObjectType>
    where PooledObjectType : class
{
    [SerializeField]
	[Tooltip("Collection checks will throw errors if we try to release an item that is already in the pool")]
    private bool collectionCheck = true;

    [field: SerializeField]
    public int MaxPoolSize { get; private set; } = 100;

	public ObjectPool<PooledObjectType> MainPool { get; private set; }


	// Initialize
	protected virtual void Awake()
	{
		MainPool = new ObjectPool<PooledObjectType>(OnCreatePooledObject, OnGetPooledObject, OnReleasePooledObject, OnDestroyPooledObject, collectionCheck, 10, MaxPoolSize);
	}

	protected abstract PooledObjectType OnCreatePooledObject();

	protected virtual void OnGetPooledObject(PooledObjectType pooledObject)
	{
		if (pooledObject is IPooledObject<PooledObjectType> foundObject)
			foundObject.OnTakenFromPool(this);
	}


	// Update
	public PooledObjectType Get()
		=> MainPool.Get();

	public object GetUnknown()
		=> Get();

	public PooledObjectType Get(Vector2 worldPosition2D)
	{
		var pooledObject = MainPool.Get();

		if (pooledObject is MonoBehaviour pooledMonoBehaviour)
			pooledMonoBehaviour.transform.position = worldPosition2D;
		else
			Debug.LogErrorFormat("{0} is not a type of MonoBehaviour. Returned normal pooled object", typeof(PooledObjectType));

		return pooledObject;
	}

	public PooledObject<PooledObjectType> Get(out PooledObjectType pooledObject)
		=> MainPool.Get(out pooledObject);

	public PooledObject<PooledObjectType> Get(Vector2 worldPosition2D, out PooledObjectType pooledObject)
	{
		var disposablePooledObject = MainPool.Get(out PooledObjectType takenPooledObject);
		pooledObject = takenPooledObject;

		if (takenPooledObject is MonoBehaviour pooledMonoBehaviour)
			pooledMonoBehaviour.transform.position = worldPosition2D;
		else
			Debug.LogErrorFormat("{0} is not a type of MonoBehaviour. Returned normal pooled object", typeof(PooledObjectType));

		return disposablePooledObject;
	}

	public void Release(PooledObjectType obj)
		=> MainPool.Release(obj);

	public void ReleaseUnknown(object obj)
		=> Release(obj as PooledObjectType);

	public void Clear()
		=> MainPool.Clear();


	// Dispose
	protected virtual void OnDestroy()
	{
		Clear();
	}

	protected abstract void OnDestroyPooledObject(PooledObjectType pooledObject);

	protected virtual void OnReleasePooledObject(PooledObjectType pooledObject)
	{
		if (pooledObject is IPooledObject<PooledObjectType> foundObject)
			foundObject.OnReleaseToPool(this);
	}
}


#if UNITY_EDITOR

public abstract partial class MonoBehaviourPoolBase<PooledObjectType>
{ }

#endif