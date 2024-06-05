using System;
using System.Collections;
using UnityEngine;

public sealed partial class BabyFoxAI : GroundedAIBase, IAIHomeAccesser
{
	[Header("Attack")]
	#region

	[SerializeField]
	private Timer normalAttackTimer = new(1f, 2f);

	private ValueTuple<IAITarget, Coroutine> normalAttTargetRoutineTuple;

	private bool IsDoingNormalAttack => (normalAttTargetRoutineTuple.Item2 != null);

	#endregion

	[field: NonSerialized]
	public bool OpenAIHomeGate { get; private set; }


	// Update
	protected override void DoIdle()
	{
		TrySetDestinationToNearestFoxBase();
	}

	public void OnEnteredAIHome(AIHomeBase home)
	{
		OpenAIHomeGate = false;
		ReleaseOrDestroySelf();
	}

	public void OnLeftFromAIHome(AIHomeBase home)
	{ }

	public bool TrySetDestinationToNearestFoxBase()
	{
		if(TagObject.TryGetNearestTagObject(this.transform, Tags.FoxAIHome, out Transform nearestFoxBase,
			(iteratedTagObject) => IsAbleToGoTo(iteratedTagObject.position)))
		{
			OpenAIHomeGate = true;
			SetDestinationTo(nearestFoxBase, 0.1f);
			return true;
		}

		return false;
	}


	private void RefreshAttackState()
	{
		ClearDestination();

		if (IsDoingNormalAttack)
			StopCoroutine(normalAttTargetRoutineTuple.Item2);

		State = AIState.Running;
		normalAttTargetRoutineTuple = default;
		normalAttackTimer.Reset();
	}

	private IEnumerator OnNormalAttack(IAITarget target)
	{
		// Take full control over the body by setting state to attacking, ready timer
		State = AIState.Attacking;

		// Wait until timer finishes and set the destination to the target recursively
		var targetTransform = (target as Component).transform;
		while (!normalAttackTimer.Tick())
		{
			yield return null;

			if (target.IsDead)
			{
				RefreshAttackState();
				yield break;
			}

			SetDestinationTo(targetTransform);
			DoRunning();
		}

		// Do the attack
		target.TakeDamage(this.Power);

		// If target dead, go try to go nearest base
		// If target not dead, repeat the attack recursively
		if (target.IsDead)
		{
			TrySetDestinationToNearestFoxBase();
			RefreshAttackState();
		}
		else
			StartCoroutine(OnNormalAttack(target));
	}

	public void OnNormalAttackTriggerEnter2D(Collider2D collider)
	{
		// If no normal attack is going on, found the target and found target is not the same type as self, do the attack
		if (!IsDoingNormalAttack
			&& EventReflector.TryGetComponentByEventReflector<IAITarget>(collider.gameObject, out IAITarget foundTarget)
			&& (foundTarget is not BabyFoxAI))
		{
			normalAttTargetRoutineTuple.Item1 = foundTarget;
			normalAttTargetRoutineTuple.Item2 = StartCoroutine(OnNormalAttack(foundTarget));
		}
	}

	public void OnNormalAttackTriggerExit2D(Collider2D collider)
	{
		if (EventReflector.TryGetComponentByEventReflector<IAITarget>(collider.gameObject, out IAITarget escapedTarget))
		{
			if (escapedTarget == normalAttTargetRoutineTuple.Item1)
				RefreshAttackState();
		}
	}

	public override void CopyTo(in AIBase main)
	{
		if (main is BabyFoxAI babyFoxAI)
			babyFoxAI.normalAttackTimer = this.normalAttackTimer;

		base.CopyTo(main);
	}
}


#if UNITY_EDITOR

public sealed partial class BabyFoxAI
{ }

#endif