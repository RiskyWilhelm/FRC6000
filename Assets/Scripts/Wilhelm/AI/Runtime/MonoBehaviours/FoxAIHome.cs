using UnityEngine;

public sealed partial class FoxAIHome : AIHomeBase
{
	[Header("FoxAIHome Spawn")]
	#region

	[SerializeField]
	private Timer spawnTimer = new(10f);


	#endregion


	// Update
	private void Update()
	{
		if ((DayCycleControllerSingleton.Instance.Time.daylightType is DaylightType.Night) && spawnTimer.Tick())
			base.TrySpawn(out _);
	}

	public void OnGateTriggerStay2D(Collider2D collider)
	{
		// Check if it is an IAIHomeAcesser then check if acesser wants to enter
		if (EventReflectorUtils.TryGetComponentByEventReflector<IAIHomeAccesser>(collider.gameObject, out IAIHomeAccesser foundAccesser))
		{
			// Check if accesser wants to go in
			if (!foundAccesser.OpenAIHomeGate)
				return;

			// Accept only FoxAI to enter
			if ((foundAccesser.ParentHome == this) && acceptedTargetTypeList.Contains(foundAccesser.TargetTag))
				foundAccesser.OnEnteredAIHome(this);
		}
	}
}


#if UNITY_EDITOR

public sealed partial class FoxAIHome
{ }

#endif