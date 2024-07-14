using UnityEngine;
using UnityEngine.Events;

public abstract partial class CarryableMonoBehaviourBase<RelativeType, CarrierRelativeType> : MonoBehaviour
    where RelativeType : Component
    where CarrierRelativeType : Component
{
	#region CarrierMonoBehaviourBase Carry

	public CarrierMonoBehaviourBase<CarrierRelativeType, RelativeType> Carrier { get; private set; }


	#endregion

	#region CarrierMonoBehaviourBase Events

	[SerializeField]
	private UnityEvent<CarrierMonoBehaviourBase<CarrierRelativeType, RelativeType>> onCarried = new();

	[SerializeField]
	private UnityEvent<CarrierMonoBehaviourBase<CarrierRelativeType, RelativeType>> onUncarried = new();


	#endregion


	// Update
	public virtual void OnCarried(CarrierMonoBehaviourBase<CarrierRelativeType, RelativeType> carrier)
	{
		Carrier = carrier;
		onCarried?.Invoke(carrier);
	}

	public virtual void OnUncarried(CarrierMonoBehaviourBase<CarrierRelativeType, RelativeType> carrier)
	{
		Carrier = null;
		onUncarried?.Invoke(carrier);
	}
}


#if UNITY_EDITOR

public abstract partial class CarryableMonoBehaviourBase<RelativeType, CarrierRelativeType>
{ }

#endif