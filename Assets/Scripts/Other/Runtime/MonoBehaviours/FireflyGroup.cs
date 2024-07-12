using UnityEngine;

public sealed partial class FireflyGroup : MonoBehaviour, IPooledObject<FireflyGroup>
{
	[SerializeField]
	private ParticleSystem selfParticleSystem;

	[SerializeField]
	private TimerRandomized destroyTimer;

	public IPool<FireflyGroup> ParentPool { get; set; }


	// Initialize
	private void OnEnable()
	{
		destroyTimer.ResetAndRandomize();
	}

	public void OnTakenFromPool(IPool<FireflyGroup> pool)
	{
		selfParticleSystem.Play();
	}


	// Update
	private void Update()
	{
		if (destroyTimer.Tick())
		{
			if (ParentPool != null)
				ParentPool.Release(this);
			else
				Destroy(this.gameObject);
		}
	}


	// Dispose
	public void OnReleaseToPool(IPool<FireflyGroup> pool)
	{
		selfParticleSystem.Stop();
	}
}


#if UNITY_EDITOR

public sealed partial class FireflyGroup
{ }

#endif