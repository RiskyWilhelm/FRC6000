public interface IAIHomeAccesser
{
	public TargetType TargetTag { get; }

	public AIHomeBase ParentHome { get; set; }

	public bool OpenAIHomeGate { get; }

	public void OnEnteredAIHome(AIHomeBase home);

	public void OnLeftFromAIHome(AIHomeBase home);
}