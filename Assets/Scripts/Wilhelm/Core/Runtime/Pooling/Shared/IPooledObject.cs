public interface IPooledObject<PooledObjectType>
	where PooledObjectType : class
{
	public IPool<PooledObjectType> ParentPool { get; set; }
}