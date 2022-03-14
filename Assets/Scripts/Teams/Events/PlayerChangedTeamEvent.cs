using Utils.Core.Events;

public class PlayerChangedTeamEvent : IEvent
{
    public readonly IPlayer Player;
    public readonly Team Team;

    public PlayerChangedTeamEvent(IPlayer player, Team team)
    {
        Player = player;
        Team = team;
    }
}
