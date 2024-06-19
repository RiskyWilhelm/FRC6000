using System;
using UnityEngine;

public sealed partial class ChickenAIHome : AIHomeBase, ITarget
{
	[Header("ChickenAIHome Spawn")]
	#region

	[SerializeField]
	private Timer lightSpawnTimer = new(10f, 10f, 20f);


	#endregion

	#region ChickenAIHome Stats

	[SerializeField]
	private Timer restoreHealthTimer = new(5f, 0f, 5f);

	[field: SerializeField]
	public uint Health { get; private set; }

	[field: SerializeField]
	public uint MaxHealth { get; private set; }

	public bool IsDead { get; private set; }

	public TargetType TargetTag => TargetType.ChickenHome;
	

	#endregion


	// Update
	private void Update()
	{
		if ((DayCycleControllerSingleton.Instance.Time.daylightType is DaylightType.Light) && lightSpawnTimer.Tick())
			base.TrySpawn(out _);

		if (IsDead && restoreHealthTimer.Tick())
			RestoreHealth();
	}

	public void OnGateTriggerStay2D(Collider2D collider)
	{
		if (EventReflectorUtils.TryGetComponentByEventReflector<IAIHomeAccesser>(collider.gameObject, out IAIHomeAccesser foundAccesser)
			&& foundAccesser.OpenAIHomeGate)
		{
			// Accept only ChickenAI to enter
			if ((foundAccesser.ParentHome == this) && acceptedTargetTypeList.Contains(foundAccesser.TargetTag))
				foundAccesser.OnEnteredAIHome(this);
		}
	}

	public void TakeDamage(uint damage, Vector2 occurWorldPosition)
	{
		Health = (ushort)Math.Clamp(Health - (int)damage, ushort.MinValue, ushort.MaxValue);

		if ((Health == ushort.MinValue) && !IsDead)
			OnDead();
	}

	private void RestoreHealth()
	{
		Health = MaxHealth;
		IsDead = false;
	}

	private void OnDead()
	{
		Debug.Log("Decreased extinction rate");

		// TODO: Decrease the extinction rate of Chickens
		IsDead = true;
	}

	public void TakeDamage(uint damage, Vector3 hitDirection)
	{
		Health = (ushort)Math.Clamp(Health - (int)damage, ushort.MinValue, ushort.MaxValue);
	}
}


#if UNITY_EDITOR

public sealed partial class ChickenAIHome
{ }

#endif