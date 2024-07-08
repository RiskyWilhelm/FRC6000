// TODO: We shouldnt forget that IInteractable is also a component. We should check via UnityEngine.Object bool operator...
public interface IInteractable
{
	public void Interact(IInteractor interactor, InteractionArgs receivedValue, out InteractionArgs resultValue);
}