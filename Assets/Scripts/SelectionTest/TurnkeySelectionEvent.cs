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
	public int volumeID = 0;
	public bool isInVolume = false;

	public TurnkeySelectionEvent(int selectionValue, bool isInVolume)
	{
		this.volumeID = selectionValue;
		this.isInVolume = isInVolume;
	}
}