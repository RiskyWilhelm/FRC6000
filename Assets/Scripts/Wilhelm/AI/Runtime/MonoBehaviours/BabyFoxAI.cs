using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

public sealed partial class BabyFoxAI : GroundedAIBase, IAIHomeAccesser
{
	[Header("BabyFoxAI Attack")]
	#region

	[SerializeField]
	private Timer normalAttackTimer = new(0.5f, 0.75f);

	private ValueTuple<IAITarget, Coroutine> normalAttTargetRoutineTuple;

	#endregion

	[Header("BabyFoxAI Enemy")]
	#region

	public List<string> ignoreEnemyTagList = new ();

	private readonly HashSet<IAITarget> enemyInRangeSet = new();

	#endregion

	#region Other

	[field: NonSerialized]
	public bool IsCaughtMeal {  get; private set; }

	[field: NonSerialized]
	public bool OpenAIHomeGate { get; private set; }

	#endregion


	// Initialize
	private void Start()
	{
		DayCycleControllerSingleton.Instance.onDaylightTypeChanged.AddListener(OnDaylightTypeChanged);
		UpdateByDaylightType(DayCycleControllerSingleton.Instance.Time.daylightType);
	}

	protected override void OnEnable()
	{
		IsCaughtMeal = false;
		OpenAIHomeGate = false;
		ClearDestination();
		RefreshAttackState();

		base.OnEnable();
	}


	// Update
	protected override void DoIdle()
	{
		TrySetDestinationToNearestFoxBase();
	}

	public void OnEnteredAIHome(AIHomeBase home)
	{
		ReleaseOrDestroySelf();
	}

	public void OnLeftFromAIHome(AIHomeBase home)
	{ }

	protected override void OnChangedDestination(Vector2? newDestination)
	{
		OpenAIHomeGate = false;
		base.OnChangedDestination(newDestination);
	}

	public bool TrySetDestinationToNearestFoxBase()
	{
		if(TagObject.TryGetNearestTagObject(this.transform, Tags.FoxAIHome, out Transform nearestFoxHome, (iteratedTagObject) => IsAbleToGoTo(iteratedTagObject.position)))
		{
			SetDestinationTo(nearestFoxHome, 0.1f);
			OpenAIHomeGate = true;
			return true;
		}

		return false;
	}

	// OPTIMIZATION: Those two called too frequently. Some of them implemented in FoxAIHome with exact topic
	public bool TrySetDestinationAwayFromPowerfulEnemyInRange()
	{
		using (ListPool<Transform>.Get(out List<Transform> powerfulEnemyInRangeList))
		{
			// Find powerful enemies in range
			foreach (var iteratedEnemy in enemyInRangeSet)
			{
				if ((iteratedEnemy is AIBase iteratedEnemyAI) && iteratedEnemyAI.IsPowerfulThan(this))
					powerfulEnemyInRangeList.Add(iteratedEnemyAI.transform);
			}

			// Try set destination away from the nearest strongest enemy
			if (powerfulEnemyInRangeList.Count > 0)
			{
				if (this.transform.TryGetNearestTransform(powerfulEnemyInRangeList, out Transform nearestPowerfulEnemy, IsCanCatchSelf))
				{
					SetDestinationToAwayFrom(nearestPowerfulEnemy.position, isGroundedOnly: true);
					return true;
				}
			}
		}

		return false;
		bool IsCanCatchSelf(Transform chaserTransform)
		{
			var chaserAIComponent = chaserTransform.GetComponent<AIBase>();
			return !chaserAIComponent.IsDead && chaserAIComponent.IsAbleToGoTo(this.transform.position);
		}
	}

	public bool TrySetDestinationToNearestEnemyInRange()
	{
		using (ListPool<Transform>.Get(out List<Transform> enemyInRangeTransformList))
		{
			// Convert enemies in range list to transform list
			foreach (var iteratedEnemy in enemyInRangeSet)
				enemyInRangeTransformList.Add((iteratedEnemy as Component).transform);

			// Set destination to the nearest enemy if able to go
			if (this.transform.TryGetNearestTransform(enemyInRangeTransformList, out Transform nearestEnemy, IsCatchable))
			{
				SetDestinationTo(nearestEnemy);
				return true;
			}
		}

		return false;
		bool IsCatchable(Transform targetTransform) => !targetTransform.GetComponent<IAITarget>().IsDead && IsAbleToGoTo(targetTransform.position);
	}

	private void RefreshAttackState()
	{
		if (State == AIState.Attacking)
		{
			if (normalAttTargetRoutineTuple.Item2 != null)
				StopCoroutine(normalAttTargetRoutineTuple.Item2);
			
			State = AIState.Idle;
		}

		normalAttTargetRoutineTuple = default;
		normalAttackTimer.Reset();
	}

	/// <summary> Attacks single target </summary>
	private IEnumerator DoNormalAttack(IAITarget target)
	{
		// Cant attack the dead
		if (target.IsDead)
			yield break;

		// Take full control over the body by setting state to attacking, ready timer
		State = AIState.Attacking;

		// Do the attack
		target.TakeDamage(this.Power);

		if (target.IsDead)
		{
			RefreshAttackState();
			OnKilledTarget(target);
			yield break;
		}

		// Lock the state to Attacking until timer ends
		while (!normalAttackTimer.Tick())
			yield return null;

		RefreshAttackState();
	}

	private void OnKilledTarget(IAITarget target)
	{
		var targetComponent = (target as Component);

		// If target is a chicken or chicken home, it means it caught a chicken
		if (targetComponent.CompareTag(Tags.ChickenAI) || targetComponent.CompareTag(Tags.ChickenAIHome))
			IsCaughtMeal = true;

		TrySetDestinationToNearestFoxBase();
	}

	public void OnNormalAttackTriggerStay2D(Collider2D collider)
	{
		// If reflected, get reflected object
		EventReflector.TryGetReflectedGameObject(collider.gameObject, out GameObject reflected);

		if (ignoreEnemyTagList.Contains(reflected.tag))
			return;

		// If currently not attacking, do the attack
		if (!(State == AIState.Attacking)
			&& reflected.TryGetComponent<IAITarget>(out IAITarget foundTarget))
		{
			normalAttTargetRoutineTuple.Item1 = foundTarget;
			normalAttTargetRoutineTuple.Item2 = StartCoroutine(DoNormalAttack(foundTarget));
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

	public void OnEnemyTriggerStay2D(Collider2D collider)
	{
		// If reflected, get reflected object
		EventReflector.TryGetReflectedGameObject(collider.gameObject, out GameObject reflected);

		if (ignoreEnemyTagList.Contains(reflected.tag))
			return;

		// If the GameObject is a AI Target, add to range list
		if (reflected.TryGetComponent<IAITarget>(out IAITarget foundTarget))
		{
			enemyInRangeSet.Add(foundTarget);

			if (State is AIState.Attacking)
				return;

			// If there is any powerful enemy in range, runaway from it and discard other targets
			if (TrySetDestinationAwayFromPowerfulEnemyInRange())
				return;

			// If didnt caught any meal, try catch the nearest enemy
			// If cant catch any enemy, set destination to home back
			if (!IsCaughtMeal && !TrySetDestinationToNearestEnemyInRange())
				TrySetDestinationToNearestFoxBase();
		}
	}

	public void OnEnemyTriggerExit2D(Collider2D collider)
	{
		// If reflected, get reflected object
		EventReflector.TryGetReflectedGameObject(collider.gameObject, out GameObject reflected);

		if (ignoreEnemyTagList.Contains(reflected.tag))
			return;

		// If the GameObject is a AI Target, remove from the range list
		if (reflected.TryGetComponent<IAITarget>(out IAITarget foundTarget))
		{
			enemyInRangeSet.Remove(foundTarget);

			// If there is no enemy in range, set destination to nearest home
			if (enemyInRangeSet.Count == 0)
				TrySetDestinationToNearestFoxBase();
		}
	}

	private void OnDaylightTypeChanged(DaylightType newDaylightType)
	{
		UpdateByDaylightType(newDaylightType);
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

	public override void CopyTo(in AIBase main)
	{
		if (main is BabyFoxAI babyFoxAI)
		{
			babyFoxAI.normalAttackTimer = this.normalAttackTimer;
			babyFoxAI.ignoreEnemyTagList = this.ignoreEnemyTagList;
		}

		base.CopyTo(main);
	}


	// Dispose
	private void OnDestroy()
	{
		if (!this.gameObject.scene.isLoaded) return;

		DayCycleControllerSingleton.Instance.onDaylightTypeChanged.RemoveListener(OnDaylightTypeChanged);
	}
}


#if UNITY_EDITOR

public sealed partial class BabyFoxAI
{ }

#endif