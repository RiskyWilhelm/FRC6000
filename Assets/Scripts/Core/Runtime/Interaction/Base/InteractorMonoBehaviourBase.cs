using UnityEngine;

public abstract partial class InteractorMonoBehaviourBase<RelativeType> : MonoBehaviour
	where RelativeType : Component
{
	[Header("InteractorMonoBehaviourBase Interaction")]
	#region InteractorMonoBehaviourBase Interaction

	[SerializeField]
	private RelativeType _relativeTo;

	public bool isInteractionBlocked;

	public RelativeType RelativeTo => _relativeTo;


	#endregion


	// Update
	public virtual bool InteractWith<InteractableType>(InteractableType interactable, InteractionType interactionType)
		where InteractableType : InteractableMonoBehaviourBase<Component, RelativeType, InteractorMonoBehaviourBase<RelativeType>>
	{
		if (!isInteractionBlocked)
			return interactable.Interact(this, interactionType);

		return false;
	}
}


#if UNITY_EDITOR

public abstract partial class InteractorMonoBehaviourBase<RelativeType>
{ }

#endif