public sealed partial class TMP_GameTimeTextUpdater : TMP_TextUpdater
{
    // Update
    public void UpdateText(GameTime gameTime)
    {
        textField.text = gameTime.ToString();
    }
}


#if UNITY_EDITOR

public sealed partial class TMP_GameTimeTextUpdater
{ }

#endif