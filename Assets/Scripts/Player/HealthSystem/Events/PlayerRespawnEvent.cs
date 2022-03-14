using Utils.Core.Events;

public class PlayerRespawnEvent : IEvent
{
	public readonly IPlayer Player = null;
	public readonly float Health;

	public PlayerRespawnEvent(IPlayer player, float health)
	{
		Player = player;
		Health = health;
	}
}
