using System;
using UnityEngine;
using Utils.Core.Injection;
using Utils.Core.Services;

public class DummyPlayer : IPlayer
{
    public bool IsLocal => false;
    public string Name { get; private set; }
    public Team Team { get; private set; }
    public string ID { get; private set; }
    public PlayerStatistics Stats { get; private set; }
    public DependencyInjector Injector { get; private set; }
    public PlayerPawn Pawn { get; protected set; }
    public event IPlayer.NameChangeDelegate NameChangeEvent;
    public event IPlayer.TeamChangeDelegate TeamChangeEvent;

    private readonly TeamsManager teamManager;

    public DummyPlayer(DependencyInjector injector, string id, bool instantiatePawn)
    {
        Injector = injector;
        Injector.RegisterInstance<IPlayer>(this);

        ID = id;
        Stats = new PlayerStatistics();
        SetName("DummyPlayer");

        teamManager = GlobalServiceLocator.Instance.Get<TeamsManager>();
        SetTeam(Team.None);

        if (instantiatePawn)
            SetPlayerPawn(InstantiatePlayerPawn());    
    }

    public void SetName(string newName)
    {
        string oldName = Name;
        Name = newName;
        NameChangeEvent?.Invoke(oldName, newName);
    }

    protected virtual PlayerPawn InstantiatePlayerPawn()
    {
        PlayerPawn pawn = Injector.InstantiateGameObject(Resources.Load<PlayerPawn>("DummyPlayer"));
        pawn.Setup(this);
        return pawn;
    }

    public void SetPlayerPawn(PlayerPawn pawn)
    {
        Pawn = pawn;
    }

    public void SetTeam(Team newTeam)
    {
        if (newTeam != Team)
        {
            Team oldTeam = Team;
            Team = newTeam;
            teamManager.ChangeTeam(this, oldTeam, newTeam);
            TeamChangeEvent?.Invoke(oldTeam, newTeam);
        }
    }

    public virtual void Dispose()
    {
        Stats.Dispose();

        if (Pawn != null)
            UnityEngine.Object.Destroy(Pawn.gameObject);
    }
}
