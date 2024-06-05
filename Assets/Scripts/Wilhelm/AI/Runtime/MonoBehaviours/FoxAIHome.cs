using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public sealed partial class FoxAIHome : AIHomeBase
{
	private readonly List<IAITarget> targetInRangeList = new();

	public string[] ignoreTargetTagArray = new string[0];


	// Update
	public void OnGateTriggerStay2D(Collider2D collider)
	{
		if (EventReflector.TryGetComponentByEventReflector<IAIHomeAccesser>(collider.gameObject, out IAIHomeAccesser foundAccesser)
			&& foundAccesser.OpenAIHomeGate)
		{
			foundAccesser.OnEnteredAIHome(this);
		}
	}

	public void OnEnemyTriggerEnter2D(Collider2D collider)
	{
		EventReflector.TryGetReflectedGameObject(collider.gameObject, out GameObject reflected);

		if (reflected.TryGetComponent<IAITarget>(out IAITarget foundTarget))
		{
			if (ignoreTargetTagArray.Contains(reflected.tag))
				return;

			targetInRangeList.Add(foundTarget);
		}
	}

	public void OnEnemyTriggerExit2D(Collider2D collider)
	{
		EventReflector.TryGetReflectedGameObject(collider.gameObject, out GameObject reflected);

		if (reflected.TryGetComponent<IAITarget>(out IAITarget foundTarget))
		{
			if (ignoreTargetTagArray.Contains(reflected.tag))
				return;

			targetInRangeList.Remove(foundTarget);
		}
	}

	protected override bool TrySpawn(out AIBase spawnedAI)
	{
		spawnedAI = null;

		// If there is any enemy nearby in range, then then tryspawn
		if ((targetInRangeList.Count > 0) && base.TrySpawn(out spawnedAI))
			spawnedAI.SetDestinationTo((targetInRangeList[0] as Component).transform);

		return spawnedAI != null;
	}
}


#if UNITY_EDITOR

public sealed partial class FoxAIHome
{ }

#endif