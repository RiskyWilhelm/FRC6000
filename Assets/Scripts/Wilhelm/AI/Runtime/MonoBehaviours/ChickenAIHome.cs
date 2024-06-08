using System;
using UnityEngine;

public sealed partial class ChickenAIHome : AIHomeBase, IAITarget
{
	[Header("ChickenAIHome Spawn")]
	#region

	[SerializeField]
	private Timer lightSpawnTimer = new(10f);

	#endregion

	[Header("ChickenAIHome Stats")]
	#region

	[SerializeField]
	private ushort _health = 10;

	[SerializeField]
	private Timer restoreHealthTimer = new(5f);

	private ushort restoreHealthOnDeath;

	public ushort Health
	{
		get => _health;
		set
		{
			restoreHealthOnDeath = value;
			_health = value;
		}
	}

	public ushort Power => 0;

	public bool IsDead { get; private set; }

	#endregion

	// Initialize
	private void OnEnable()
	{
		restoreHealthOnDeath = Health;
	}


	// Update
	private void Update()
	{
		if ((DayCycleControllerSingleton.Instance.Time.daylightType is DaylightType.Light) && lightSpawnTimer.Tick())
			TrySpawn(out _);

		if (IsDead && restoreHealthTimer.Tick())
			RestoreHealth();
	}

	public void OnGateTriggerStay2D(Collider2D collider)
	{
		if (EventReflector.TryGetComponentByEventReflector<IAIHomeAccesser>(collider.gameObject, out IAIHomeAccesser foundAccesser)
			&& foundAccesser.OpenAIHomeGate)
		{
			// Accept only ChickenAI to enter
			var foundAccesserComponent = (foundAccesser as Component);

			if (foundAccesserComponent.CompareTag(Tags.ChickenAI))
				foundAccesser.OnEnteredAIHome(this);
		}
	}

	public void TakeDamage(uint damage)
	{
		_health = (ushort)Math.Clamp(_health - (int)damage, ushort.MinValue, ushort.MaxValue);

		if (_health == ushort.MinValue && !IsDead)
			OnDead();
	}

	private void RestoreHealth()
	{
		_health = restoreHealthOnDeath;
		IsDead = false;
	}

	private void OnDead()
	{
		Debug.Log("Decreased extinction rate");

		// TODO: Decrease the extinction rate of Chickens
		IsDead = true;
	}
}


#if UNITY_EDITOR

public sealed partial class ChickenAIHome
{ }

#endif