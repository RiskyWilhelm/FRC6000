// TODO: We shouldnt forget that IInteractable is also a component. We should check via UnityEngine.Object bool operator...
public interface IInteractor
{
	public void InteractWith(IInteractable interactable);
}