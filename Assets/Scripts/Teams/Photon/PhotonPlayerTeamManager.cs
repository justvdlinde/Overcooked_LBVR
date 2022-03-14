using ExitGames.Client.Photon;
using Photon.Realtime;
using System;

public class PhotonPlayerTeamManager : IDisposable
{
    public Team Team { get; private set; } = Team.None;

    private readonly PhotonPlayer player;
    private readonly PhotonNetworkedPlayer photonClient;
    private readonly TeamsManager teamManager;

    public PhotonPlayerTeamManager(IPlayer player, TeamsManager teamManager)
    {
        this.player = player as PhotonPlayer;
        photonClient = this.player.NetworkClient;
        this.teamManager = teamManager;

        if (player.IsLocal)
        {
            photonClient.SetCustomProperty(PlayerPropertiesPhoton.TEAM, (byte)Team);
        }
        else
        { 
            if (photonClient.GetCustomProperty(PlayerPropertiesPhoton.TEAM, out object team))
                SetTeam((Team)team);
        }

        this.player.PropertiesChangedEvent += OnPropertiesChangedEvent;
    }

    public void Dispose()
    {
        player.PropertiesChangedEvent -= OnPropertiesChangedEvent;
    }

    public void SetTeam(Team newTeam)
    {
        Team oldTeam = Team;
        Team = newTeam;
        teamManager.ChangeTeam(player, oldTeam, newTeam);
        photonClient.SetCustomProperty(PlayerPropertiesPhoton.TEAM, (byte)newTeam);
    }

    private void OnPropertiesChangedEvent(Hashtable properties)
    {
        if (properties.TryGetValue(PlayerPropertiesPhoton.TEAM, out object newTeam))
        {
            if(Team != (Team)newTeam)
                SetTeam((Team)newTeam);
        }
    }
}
