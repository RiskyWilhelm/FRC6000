public interface IAIHomeAccesser 
{
	public bool OpenAIHomeGate { get; }

	public void OnEnteredAIHome(AIHomeBase home);

	public void OnLeftFromAIHome(AIHomeBase home);
}