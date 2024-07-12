using UnityEngine.Pool;

public interface IPool
{
	public object GetUnknown();

	public void ReleaseUnknown(object obj);
}

public interface IPool<PooledObjectType> : IPool
	where PooledObjectType : class
{
	public PooledObjectType Get();

	public PooledObject<PooledObjectType> Get(out PooledObjectType pooledObject);

	public void Release(PooledObjectType obj);

	public void Clear();
}