using System;
using UnityEngine;

public abstract partial class InteractableMonoBehaviourBase<RelativeType, InteractorRelativeType, InteractorType> : MonoBehaviour
	where InteractorType : InteractorMonoBehaviourBase<InteractorRelativeType>
	where InteractorRelativeType : Component
{
	[Header("InteractableMonoBehaviourBase Interaction")]
	#region InteractableMonoBehaviourBase Interaction

	[SerializeField]
	private RelativeType _relativeTo;

	[NonSerialized]
	public InteractorType currentInteractor;

	public bool isInteractionBlocked;

	public RelativeType RelativeTo => _relativeTo;


	#endregion

	[Header("InteractableMonoBehaviourBase Visuals")]
	#region InteractableMonoBehaviourBase Visuals

	[SerializeField]
	[Tooltip("Optional")]
	private Transform interactionVisualsRoot;


	#endregion


	// Update
	protected virtual void Update()
	{
		ShowInteractionUIIfNeeded();
	}

	public abstract bool Interact(InteractorType interactor, InteractionType interactionType);

	public bool InteractWithCurrent(InteractionType interactionType)
		=> Interact(currentInteractor, interactionType);

	private void ShowInteractionUIIfNeeded()
	{
		if (currentInteractor && !currentInteractor.isInteractionBlocked)
			ShowInteractionUI();
		else if (!currentInteractor || currentInteractor.isInteractionBlocked)
			HideInteractionUI();
	}

	public virtual void ShowInteractionUI()
	{
		if (interactionVisualsRoot)
			interactionVisualsRoot.gameObject.SetActive(true);
	}

	public virtual void HideInteractionUI()
	{
		if (interactionVisualsRoot)
			interactionVisualsRoot.gameObject.SetActive(false);
	}

	public void OnUICarryClick_InteractWith(InteractorType interactor)
		=> Interact(interactor, InteractionType.Carry);

	public void OnUICarryClick_InteractWithCurrent()
		=> InteractWithCurrent(InteractionType.Carry);

	public void OnUIUnCarryClick_InteractWith(InteractorType interactor)
		=> Interact(interactor, InteractionType.UnCarry);

	public void OnUIUnCarryClick_InteractWithCurrent()
		=> InteractWithCurrent(InteractionType.UnCarry);

	public void OnUIKillClick_InteractWith(InteractorType interactor)
		=> Interact(interactor, InteractionType.KillTarget);

	public void OnUIKillClick_InteractWithCurrent()
		=> InteractWithCurrent(InteractionType.KillTarget);

	public void OnUIGoHomeClick_InteractWith(InteractorType interactor)
		=> Interact(interactor, InteractionType.GoHome);

	public void OnUIGoHomeClick_InteractWithCurrent()
		=> InteractWithCurrent(InteractionType.GoHome);

	public void OnInteractorTriggerEnter2D(Collider2D collider2D)
	{
		if (!collider2D)
			return;

		if (EventReflectorUtils.TryGetComponentByEventReflector<InteractorType>(collider2D.gameObject, out InteractorType found))
		{
			currentInteractor = found;
			ShowInteractionUI();
		}
	}

	public void OnInteractorTriggerExit2D(Collider2D collider2D)
	{
		if (!collider2D)
			return;

		if (EventReflectorUtils.TryGetComponentByEventReflector<InteractorType>(collider2D.gameObject, out InteractorType found))
		{
			if (currentInteractor == found)
				currentInteractor = null;

			HideInteractionUI();
		}
	}
}


#if UNITY_EDITOR

public abstract partial class InteractableMonoBehaviourBase<RelativeType, InteractorRelativeType, InteractorType>
{ }

#endif