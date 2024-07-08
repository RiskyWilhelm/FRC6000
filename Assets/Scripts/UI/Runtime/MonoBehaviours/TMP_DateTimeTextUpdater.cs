using System;

public sealed partial class TMP_DateTimeTextUpdater : TMP_TextUpdater
{
    // Update
    public void UpdateText(DateTime time, string format)
    {
        textField.text = time.ToString(format);
    }
}


#if UNITY_EDITOR

public sealed partial class TMP_DateTimeTextUpdater
{ }

#endif