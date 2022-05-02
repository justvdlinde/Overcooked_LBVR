using Photon.Realtime;
using System;
using Utils.Core.Injection;
using Utils.Core.Services;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class PhotonPlayer : PhotonClient, IPlayer
{
    public string Name { get; private set; }
    public Team Team => teamManager.Team;
    public string ID => UserId;
    public DependencyInjector Injector { get; private set; }
    public PlayerPawn Pawn { get; private set; }

    public event IPlayer.NameChangeDelegate NameChangeEvent;
    public event IPlayer.TeamChangeDelegate TeamChangeEvent;

    private readonly PhotonPlayerTeamManager teamManager;
    public Action<Hashtable> PropertiesChangedEvent;

    public PhotonPlayer(DependencyInjector injector, PhotonNetworkedPlayer networkClient, string ipAddress = "", string name = "") : base(networkClient, ipAddress)
    {
        Injector = injector;
        injector.RegisterInstance<IPlayer>(this);
        teamManager = new PhotonPlayerTeamManager(this, GlobalServiceLocator.Instance.Get<TeamsManager>());

        if (IsLocal)
        {
            networkClient.SetCustomProperty(PlayerPropertiesPhoton.CLIENT_TYPE, (byte)ClientType.Player);
            SetName(name);
            InstantiateLocalPlayerPawn();
        }
        else
        {
            if (networkClient.CustomProperties.TryGetValue(PlayerPropertiesPhoton.PLAYER_NAME, out object value))
                SetName((string)value);
        }
    }

    public void InstantiateLocalPlayerPawn()
    {
        LocalPlayerPawnFactory factory = new LocalPlayerPawnFactory(Injector);
        PlayerPawn pawn = factory.Construct();
        Injector.RegisterInstance<PlayerPawn>(pawn);
        SetPlayerPawn(pawn);
        Pawn.Setup(this);
    }

    public void SetName(string newName)
    {
        if (newName != Name)
        {
            string oldName = Name;
            Name = newName;
            NetworkClient.SetCustomProperty(PlayerPropertiesPhoton.PLAYER_NAME, Name);
            NetworkClient.NickName = newName;
            NameChangeEvent?.Invoke(oldName, newName);
        }
    }

    protected override void OnPlayerPropertiesChangedEvent(PhotonNetworkedPlayer targetPlayer, Hashtable properties)
    {
        if (targetPlayer == NetworkClient)
        {
            if (properties.TryGetValue(PlayerPropertiesPhoton.PLAYER_NAME, out object value))
            {
                if (Name != (string)value)
                {
                    SetName((string)value);
                }
            }
            PropertiesChangedEvent?.Invoke(properties);
        }
    }

    public override void Dispose()
    {
        base.Dispose();

        if (Pawn != null)
        {
            UnityEngine.Object.Destroy(Pawn.gameObject);
            Injector.UnRegisterInstance<PlayerPawn>();
        }
    }

    public void SetTeam(Team newTeam)
    {
        if (newTeam != Team)
        {
            Team oldTeam = Team;
            teamManager.SetTeam(newTeam);
            TeamChangeEvent?.Invoke(oldTeam, newTeam);
        }
    }

    public void SetPlayerPawn(PlayerPawn controller)
    {
        Pawn = controller;
    }
}
