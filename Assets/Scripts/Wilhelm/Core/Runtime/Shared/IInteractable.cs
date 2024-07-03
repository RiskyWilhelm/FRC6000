using UnityEngine;

public interface IInteractable
{
	public bool Interact(MonoBehaviour requester);
}

public interface IInteractable<in InteractorType> : IInteractable
	where InteractorType : MonoBehaviour
{
	public bool Interact(InteractorType requester);
}