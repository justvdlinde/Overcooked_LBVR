using Utils.Core.Events;

public enum SelectionType
{
	None,
	Gamemode,
	Replay,
	MapSelection,
	Gameplay
}

public class TurnkeySelectionEvent : IEvent
{
	public SelectionType eventType = SelectionType.None;
	public bool wantToExecutePositively = true;

	public TurnkeySelectionEvent(SelectionType eventType, bool wantToExecutePositively)
	{
		this.eventType = eventType;
		this.wantToExecutePositively = wantToExecutePositively;
	}
}