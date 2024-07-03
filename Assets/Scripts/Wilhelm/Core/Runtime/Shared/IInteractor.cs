using UnityEngine;

public interface IInteractor
{
	public void OnInteracted(MonoBehaviour interacted);
}

public interface IInteractor<in InteractableType> : IInteractor
	where InteractableType : MonoBehaviour
{
	public void OnInteracted(InteractableType interacted);
}