public interface IHomeAccesser
{
	public TargetType TargetTag { get; }

	public HomeBase ParentHome { get; set; }

	public bool OpenAIHomeGate { get; }

	public void OnEnteredAIHome(HomeBase home);

	public void OnLeftFromAIHome(HomeBase home);
}