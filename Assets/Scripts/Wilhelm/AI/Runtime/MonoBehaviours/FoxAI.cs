using System;
using System.Collections.Generic;
using UnityEngine;

public sealed partial class FoxAI : AIBase
{
	private readonly List<ChickenAI> caughtChickens = new ();

	// Update
	protected override void Update()
	{
		ChaseChickenOrGoBase();
		base.Update();
	}

	private void ChaseChickenOrGoBase()
	{
		if (caughtChickens.Count > 0)
			SetDestinationToNearestBase();
		else
			SetDestinationToNearestChicken();
	}

	private void SetDestinationToNearestBase()
	{
		
	}

	public void SetDestinationToNearestChicken()
	{
		if (TagObject.TryGetActiveObjectListFromTag(Tags.Chicken, out List<Transform> activeChickenList))
		{
			// Ready
			Transform nearestChicken = null;
			float nearestHorizontalDistance = Mathf.Abs(activeChickenList[0].position.x - this.transform.position.x);

			// Check distances and select nearest chicken
			foreach (var iteratedChicken in activeChickenList)
			{
				var iteratedHorizontalDistance = Mathf.Abs(iteratedChicken.position.x - this.transform.position.x);

				if ((iteratedHorizontalDistance <= nearestHorizontalDistance) && IsAbleToGo(iteratedChicken.position))
				{
					nearestChicken = iteratedChicken;
					nearestHorizontalDistance = iteratedHorizontalDistance;
				}
			}

			if (nearestChicken != null)
				SetDestinationTo(nearestChicken.position);
		}
	}

	public void OnCaughtChicken(Collider2D collider)
	{
		if (TryGetTargetFromCollider(collider, out ChickenAI caughtChicken))
		{
			caughtChickens.Add(caughtChicken);
			caughtChicken.OnGotCaughtBy(this);
		}
	}
}


#if UNITY_EDITOR

public sealed partial class FoxAI
{ }

#endif