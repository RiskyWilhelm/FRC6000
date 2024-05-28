using System.Collections;
using UnityEngine;

public sealed partial class FoxAI : AIBase<FoxAIStats>
{
	// Update
	protected override void DoIdle()
	{
		// If found nearest chicken and yet didnt caught any chicken, catch the chicken
		// Else, go to the nearest fox base
		if (!TrySetDestinationToNearestChicken())
			TrySetDestinationToNearestFoxBase();
	}

	public bool TrySetDestinationToNearestFoxBase()
	{
		if(TagObject.TryGetNearestTagObject(this.transform, Tags.FoxBase, out Transform nearestFoxBase,
			(iteratedTagObject) => IsAbleToGo(iteratedTagObject.position)))
		{
			SetDestinationTo(nearestFoxBase);
			return true;
		}

		return false;
	}

	public bool TrySetDestinationToNearestChicken()
	{
		if (!Stats.IsCaughtChicken && TagObject.TryGetNearestTagObject(this.transform, Tags.Chicken, out Transform nearestChicken,
			(iteratedTagObject) => IsAbleToGo(iteratedTagObject.position)))
		{
			SetDestinationTo(nearestChicken);
			return true;
		}

		return false;
	}

	private void CancelNormalAttack<TStatsType>(AIBase<TStatsType> target)
		where TStatsType : AIStats
	{
		State = AIState.Running;
		StopCoroutine(OnNormalAttack(target));
	}

	private IEnumerator OnNormalAttack<TStatsType>(AIBase<TStatsType> target)
		where TStatsType : AIStats
	{
		// Take full control over the body by setting state to attacking, ready timer
		State = AIState.Attacking;
		var normalAttackTimer = new Timer(this.Stats.NormalAttackSpeed);

		// Wait until timer finishes and set the destination to the target recursively
		while (!normalAttackTimer.Tick())
		{
			if (target.State == AIState.Dead)
			{
				ClearDestination();
				yield break;
			}

			SetDestinationTo(target.transform);
			DoRunning();
			yield return null;
		}

		// Do the attack
		target.TakeDamage(this.Stats.Power);

		// If target dead, go try to go nearest base
		// If target not dead, repeat the attack recursively
		if (target.State == AIState.Dead)
		{
			State = AIState.Running;
			Stats.IsCaughtChicken = true;
			TrySetDestinationToNearestFoxBase();
		}
		else
			StartCoroutine(OnNormalAttack(target));
	}

	public void OnCaughtChicken(Collider2D collider)
	{
		if (EventReflector.TryGetComponentByEventReflector<ChickenAI>(collider.gameObject, out ChickenAI caughtChicken))
			StartCoroutine(OnNormalAttack(caughtChicken));
	}

	public void OnChickenRanaway(Collider2D collider)
	{
		if (EventReflector.TryGetComponentByEventReflector<ChickenAI>(collider.gameObject, out ChickenAI escapedChicken))
			CancelNormalAttack(escapedChicken);
	}
}


#if UNITY_EDITOR

public sealed partial class FoxAI
{ }

#endif