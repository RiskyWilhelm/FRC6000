using FMOD.Studio;

public static class FMODExtensions
{
	public static bool IsPlaying(this EventInstance instance)
	{
		if (instance.isValid())
		{
			instance.getPlaybackState(out PLAYBACK_STATE playbackState);
			return (playbackState != PLAYBACK_STATE.STOPPED);
		}
		return false;
	}
}