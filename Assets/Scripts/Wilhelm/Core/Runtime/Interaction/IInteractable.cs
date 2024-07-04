// TODO: We shouldnt forget that IInteractable is also a component. We should check via UnityEngine.Object bool operator...
public interface IInteractable
{
	/// <returns> Whether the interaction was successful or not </returns>
	public void Interact(IInteractor interactor, InteractionArgs receivedValue, out InteractionArgs resultValue);
}