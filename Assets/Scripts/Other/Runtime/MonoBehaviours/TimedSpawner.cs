using AYellowpaper;
using UnityEngine;

public sealed partial class TimedSpawner : MonoBehaviour
{
	#region TimedSpawner Spawn

	[SerializeField]
	private TimerRandomized spawnTimer;

	[SerializeField]
	private InterfaceReference<IPool> spawnPool;

	[SerializeField]
	[Tooltip("Box2D and Circle2D is accepted")]
	private Collider2D spawnBoundsCollider;


	#endregion


	// Update
	private void Update()
	{
		if (spawnTimer.Tick())
		{
			var spawnedObject = spawnPool.Value.GetUnknown();

			if (spawnedObject is MonoBehaviour script)
				script.transform.position = spawnBoundsCollider.GetRandomPoint();
			else
				spawnPool.Value.ReleaseUnknown(spawnedObject);

			spawnTimer.Reset();
		}
	}
}


#if UNITY_EDITOR

public sealed partial class TimedSpawner
{ }

#endif