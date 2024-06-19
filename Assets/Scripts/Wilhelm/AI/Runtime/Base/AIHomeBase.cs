using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public abstract partial class AIHomeBase : MonoBehaviour
{
	[Header("AIHomeBase Spawn")]
	#region

	public List<LuckValue<AIPool>> luckAIPoolList = new ();


	#endregion

	#region AIBase Destination & Target Verify

	public List<TargetType> acceptedTargetTypeList = new();


	#endregion


	// Update	
	protected virtual bool TrySpawn(out AIBase spawnedAI)
	{
		spawnedAI = null;
		bool isSpawned = false;

		// Get auto-generated luck and all of the LuckAIPool elements where the LuckAIPool has the value of auto-generated luck
		var generatedLuckType = LuckUtils.Generate();
		var cachedLuckList = ListPool<LuckValue<AIPool>>.Get();

        foreach (var iteratedLuckPool in luckAIPoolList)
        {
			if (iteratedLuckPool.luckType.HasFlag(generatedLuckType))
				cachedLuckList.Add(iteratedLuckPool);
        }

		// Try Spawn and release the cached list to the pool
		if (cachedLuckList.Count > 0)
		{
			var randomSelect = UnityEngine.Random.Range(0, cachedLuckList.Count);
			
			spawnedAI = cachedLuckList[randomSelect].value.Get(this.transform.position);

			// TODO: This is ridicilous
			if (spawnedAI is IAIHomeAccesser homeAccesser)
			{
				homeAccesser.OnLeftFromAIHome(this);
				homeAccesser.ParentHome = this;
			}

			isSpawned = true;
		}

		ListPool<LuckValue<AIPool>>.Release(cachedLuckList);
		return isSpawned;
    }
}


#if UNITY_EDITOR

public abstract partial class AIHomeBase
{ }

#endif