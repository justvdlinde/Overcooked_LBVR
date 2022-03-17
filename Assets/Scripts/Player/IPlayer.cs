using System;
using Utils.Core.Injection;

public interface IPlayer : IDisposable
{
    public bool IsLocal { get; }
    public string Name { get; }
    public Team Team { get; }
    public string ID { get; }

    public delegate void NameChangeDelegate(string oldName, string newName);
    public event NameChangeDelegate NameChangeEvent;

    public delegate void TeamChangeDelegate(Team oldTeam, Team newTeam);
    public event TeamChangeDelegate TeamChangeEvent;

    public PlayerStatistics Stats { get; }
    public DependencyInjector Injector { get; }
    public PlayerPawn Pawn { get; }

    void SetName(string newName);
    void SetTeam(Team newTeam);
    void SetPlayerPawn(PlayerPawn pawn);
}
