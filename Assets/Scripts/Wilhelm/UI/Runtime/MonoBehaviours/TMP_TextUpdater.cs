using TMPro;
using UnityEngine;

public partial class TMP_TextUpdater : MonoBehaviour
{
    [SerializeField]
    protected TMP_Text textField;


    // Update
    public void UpdateText(in string str)
    {
        textField.text = str;
    }
}


#if UNITY_EDITOR

public partial class TMP_TextUpdater
{ }

#endif