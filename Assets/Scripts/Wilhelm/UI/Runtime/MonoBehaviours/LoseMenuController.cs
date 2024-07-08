using UnityEngine;
using UnityEngine.EventSystems;

public sealed partial class LoseMenuController : MonoBehaviour, IPointerClickHandler
{
	#region LoseMenuController Visuals

	[SerializeField]
	private RectTransform loseMenuRootRTransform;


	#endregion


	// Update
	public void OnPointerClick(PointerEventData eventData)
	{
		GameControllerPersistentSingleton.Instance.RestartGame();
	}
}


#if UNITY_EDITOR

public sealed partial class LoseMenuController
{ }

#endif