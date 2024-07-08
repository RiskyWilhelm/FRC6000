public interface IPooledObject<PooledObjectType>
	where PooledObjectType : class
{
	public IPool<PooledObjectType> ParentPool { get; set; }


	public void OnTakenFromPool(IPool<PooledObjectType> pool);

	public void OnReleaseToPool(IPool<PooledObjectType> pool);
}