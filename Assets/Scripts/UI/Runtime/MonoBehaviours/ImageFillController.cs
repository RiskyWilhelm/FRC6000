using UnityEngine;
using UnityEngine.UI;

public sealed partial class ImageFillController : MonoBehaviour
{
    [SerializeField]
    private Image image;

    [SerializeField]
    private float maxValue;


    // Update
    public void UpdateFillAmount(float value)
    {
		image.fillAmount = Mathf.Clamp01(value / maxValue);
	}

	public void UpdateFillAmount(uint value)
        => UpdateFillAmount((float)value);
}


#if UNITY_EDITOR

public sealed partial class ImageFillController
{ }

#endif