using System;
using UnityEngine;

public abstract partial class CarrierMonoBehaviourBase<RelativeType, CarryableRelativeType> : MonoBehaviour
    where RelativeType : Component
    where CarryableRelativeType : Component
{
	#region CarrierMonoBehaviourBase Carry

	[NonSerialized]
	private Rigidbody2D carriedRigidbody;

	[SerializeField]
	private CarryableMonoBehaviourBase<CarryableRelativeType, RelativeType> currentCarried;

	[SerializeField]
	private Transform carryPoint;

	public bool IsCarriedValid => carriedRigidbody && currentCarried;


	#endregion


	// Update
	private void Update()
	{
		UpdateCarried();
	}

	private void UpdateCarried()
	{
		if (carriedRigidbody && carriedRigidbody.gameObject.activeSelf && currentCarried.gameObject.activeSelf)
			carriedRigidbody.MovePosition(carryPoint.position);
		else
			StopCarrying();
	}

	public bool Carry(CarryableMonoBehaviourBase<CarryableRelativeType, RelativeType> carryable, Rigidbody2D carryRigidbody)
	{
		if (carryable && carryRigidbody)
		{
			StopCarrying();
			carriedRigidbody = carryRigidbody;
			currentCarried = carryable;
			carryable.OnCarried(this);
			return true;
		}

		return false;
	}

	public void StopCarrying()
	{
		if (currentCarried)
			currentCarried.OnUncarried(this);

		carriedRigidbody = default;
		currentCarried = default;
	}

	public void StopCarrying(CarryableMonoBehaviourBase<CarryableRelativeType, RelativeType> carryable, Rigidbody2D uncarryRigidbody)
	{
		var isCarriedEqualWithWanted = (currentCarried == carryable) && (carriedRigidbody == uncarryRigidbody);

		if (isCarriedEqualWithWanted || !IsCarriedValid)
			StopCarrying();
	}
}


#if UNITY_EDITOR

public abstract partial class CarrierMonoBehaviourBase<RelativeType, CarryableRelativeType>
{ }

#endif