using System.Collections.Generic;
using UnityEngine;

public sealed partial class FoxAI : AIBase
{
	private readonly List<ChickenAI> caughtChickens = new ();

	// Update
	protected override void DoIdle()
	{
		ChaseChickenOrGoBase();
	}

	public void ChaseChickenOrGoBase()
	{
		if (caughtChickens.Count > 0)
			SetDestinationToNearestFoxBase();
		else
			SetDestinationToNearestChicken();
	}

	public void SetDestinationToNearestFoxBase()
	{
		if (TagObject.TryGetNearestTagObject(this.transform, Tags.FoxBase, out Transform nearestFoxBase, (iteratedTagObject) => IsAbleToGo(iteratedTagObject.position)))
			SetDestinationTo(nearestFoxBase.position, 0.5f);
	}

	public void SetDestinationToNearestChicken()
	{
		if (TagObject.TryGetNearestTagObject(this.transform, Tags.Chicken, out Transform nearestChicken, (iteratedTagObject) => IsAbleToGo(iteratedTagObject.position)))
			SetDestinationTo(nearestChicken.position, 0.5f);
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