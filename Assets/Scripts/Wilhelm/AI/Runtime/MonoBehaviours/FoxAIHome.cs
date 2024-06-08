using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public sealed partial class FoxAIHome : AIHomeBase
{
	[Header("FoxAIHome Spawn")]
	#region

	[SerializeField]
	private Timer spawnTimer = new(10f);

	#endregion

	[Header("FoxAIHome Enemy")]
	#region

	public List<string> ignoreEnemyTagList = new();

	private readonly List<IAITarget> enemyInRangeList = new();

	#endregion


	// Initialize
	private void Start()
	{
		DayCycleControllerSingleton.Instance.onDaylightTypeChanged.AddListener(OnDaylightTypeChanged);
		UpdateByDaylightType(DayCycleControllerSingleton.Instance.Time.daylightType);
	}


	// Update
	private void Update()
	{
		if (spawnTimer.Tick() && (enemyInRangeList.Count > 0))
		{
			// If spawned but cant set the destination, release
			if (base.TrySpawn(out AIBase spawnedAI) && !TrySetDestinationToNearestEnemyInRange(spawnedAI))
			{
				if (spawnedAI is IAIHomeAccesser homeAccesser)
					homeAccesser.OnEnteredAIHome(this);
			}
		}
	}

	// OPTIMIZATION: Some of them implemented in BabyFoxAI with exact topic
	public bool TrySetDestinationToNearestEnemyInRange(AIBase spawnedAI)
	{
		using (ListPool<Transform>.Get(out List<Transform> enemyInRangeTransformList))
		{
			// Convert enemies in range list to transform list
			foreach (var iteratedEnemy in enemyInRangeList)
				enemyInRangeTransformList.Add((iteratedEnemy as Component).transform);

			// Set destination to the nearest enemy if able to go
			if (this.transform.TryGetNearestTransform(enemyInRangeTransformList, out Transform nearestEnemy, IsCatchable))
			{
				spawnedAI.SetDestinationTo(nearestEnemy);
				return true;
			}
		}

		return false;
		bool IsCatchable(Transform targetTransform) => !targetTransform.GetComponent<IAITarget>().IsDead && spawnedAI.IsAbleToGoTo(targetTransform.position);
	}

	private void OnDaylightTypeChanged(DaylightType newDaylightType)
	{
		UpdateByDaylightType(newDaylightType);
	}

	public void OnGateTriggerStay2D(Collider2D collider)
	{
		// Check if it is an IAIHomeAcesser then check if acesser wants to enter
		if (EventReflector.TryGetComponentByEventReflector<IAIHomeAccesser>(collider.gameObject, out IAIHomeAccesser foundAccesser)
			&& foundAccesser.OpenAIHomeGate)
		{
			// Accept only FoxAI to enter
			var foundAccesserComponent = (foundAccesser as Component);

			if (foundAccesserComponent.CompareTag(Tags.FoxAI))
				foundAccesser.OnEnteredAIHome(this);
		}
	}

	public void OnEnemyTriggerEnter2D(Collider2D collider)
	{
		// If reflected, get reflected object
		EventReflector.TryGetReflectedGameObject(collider.gameObject, out GameObject reflected);

		if (ignoreEnemyTagList.Contains(reflected.tag))
			return;

		// If the GameObject is a AI Target, add to the range list
		if (reflected.TryGetComponent<IAITarget>(out IAITarget foundTarget))
			enemyInRangeList.Add(foundTarget);
	}

	public void OnEnemyTriggerExit2D(Collider2D collider)
	{
		// If reflected, get reflected object
		EventReflector.TryGetReflectedGameObject(collider.gameObject, out GameObject reflected);
			
		if (ignoreEnemyTagList.Contains(reflected.tag))
			return;

		// If the GameObject is a AI Target, remove from the range list
		if (reflected.TryGetComponent<IAITarget>(out IAITarget foundTarget))
			enemyInRangeList.Remove(foundTarget);
	}

	private void UpdateByDaylightType(DaylightType newDaylightType)
	{
		switch (newDaylightType)
		{
			case DaylightType.Light:
			{
				if (!ignoreEnemyTagList.Contains(Tags.ChickenAIHome))
					ignoreEnemyTagList.Add(Tags.ChickenAIHome);
			}
			break;

			case DaylightType.Night:
			{
				ignoreEnemyTagList.Remove(Tags.ChickenAIHome);
			}
			break;
		}
	}


	// Dispose
	private void OnDestroy()
	{
		if (!this.gameObject.scene.isLoaded) return;

		DayCycleControllerSingleton.Instance.onDaylightTypeChanged.RemoveListener(OnDaylightTypeChanged);
	}
}


#if UNITY_EDITOR

public sealed partial class FoxAIHome
{ }

#endif