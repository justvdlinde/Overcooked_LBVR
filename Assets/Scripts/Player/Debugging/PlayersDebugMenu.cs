using Photon.Pun;
using System;
using System.Collections.Generic;
using UnityEngine;

public class PlayersDebugMenu : IDebugMenu
{
    private class PlayerUIWrapper
    {
        public readonly IPlayer Player;

        public int TeamIndex
        {
            get
            {
                return teamIndex;
            }
            set
            {
                if (teamIndex != value)
                {
                    teamIndex = value;
                    Player.SetTeam((Team)teamIndex);
                }
            }
        }
        private int teamIndex = 0;
        public string nameField;
        public string debugDamage = "1";
        public string respawnTimer = "1";

        public PlayerUIWrapper(IPlayer player)
        {
            this.Player = player;
            TeamIndex = (int)player.Team;
            nameField = player.Name;
        }
    }

    private readonly PlayersManager playersManager;
    private readonly TeamsManager teamsManager;
    private Vector2 scrollPosition;
    private string[] teamStrings;
    private string[] playerNames;
    private List<PlayerUIWrapper> playersUIList;

    private int killerDebugIndex;
    private IPlayer killerDebug;

    private int victimDebugIndex;
    private IPlayer victimDebug;

    public PlayersDebugMenu(PlayersManager playersManager, TeamsManager teamsManager)
    {
        this.playersManager = playersManager;
        this.teamsManager = teamsManager;

        teamStrings = Enum.GetNames(typeof(Team));
    }

    public void Open()
    {
        BuildPlayerUIlist();
        playersManager.PlayerJoinEvent += OnPlayerJoinEvent;
        playersManager.PlayerLeftEvent += OnPlayerLeftEvent;
    }

    public void Close()
    {
        playersManager.PlayerJoinEvent -= OnPlayerJoinEvent;
        playersManager.PlayerLeftEvent -= OnPlayerLeftEvent;
    }

    public void OnGUI(bool drawDeveloperOptions)
    {
        GUILayout.BeginVertical("box", GUILayout.MinWidth(400));
        DrawInfo();
        if (drawDeveloperOptions)
        {
            GUILayout.Space(5);
            DrawKillPanel();
            GUILayout.Space(5);
            DrawDummyCreatorPanel();
        }
        GUILayout.Space(5);
        DrawPlayersList(drawDeveloperOptions);
        GUILayout.EndVertical();
    }

    private void DrawInfo()
    {
        GUILayout.BeginVertical("Players", "window");
        GUILayout.Label("Player count (Photon): " + ((PhotonNetwork.CurrentRoom != null) ? PhotonNetwork.CurrentRoom.PlayerCount.ToString() : "-"));
        GUILayout.Label("Player count: " + playersManager.PlayerCount);
        GUILayout.Label("Operator count: " + playersManager.Operators.Count);
        GUILayout.Label("Spectator count: " + playersManager.Spectators.Count);
        GUILayout.Label("Local Player Type: " + (playersManager.LocalPlayer != null ? playersManager.LocalPlayer.GetType().ToString() : "-"));
        GUILayout.BeginHorizontal();
        GUILayout.Label("Team None: " + teamsManager.PlayersPerTeam[Team.None].Count);
        GUILayout.Label("FFA: " + teamsManager.PlayersPerTeam[Team.FreeForAll].Count);
        GUILayout.EndHorizontal();
        GUILayout.BeginHorizontal();
        GUILayout.Label("Team 1: " + teamsManager.PlayersPerTeam[Team.Team1].Count);
        GUILayout.Label("Team 2: " + teamsManager.PlayersPerTeam[Team.Team2].Count);
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
    }

    private void DrawKillPanel()
    {
        GUILayout.BeginVertical("Emulate Kill", "window");

        GUILayout.BeginHorizontal();
        GUILayout.Label("Killer: ", GUILayout.Width(40));
        killerDebugIndex = GUILayout.Toolbar(killerDebugIndex, playerNames, GUILayout.MaxWidth(400));
        killerDebug = playersManager.AllPlayers[killerDebugIndex];
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUILayout.Label("victim: ", GUILayout.Width(40));
        victimDebugIndex = GUILayout.Toolbar(victimDebugIndex, playerNames, GUILayout.MaxWidth(400));
        victimDebug = playersManager.AllPlayers[victimDebugIndex];
        GUILayout.EndHorizontal();

        if (killerDebug != null && victimDebug != null)
        {
            GUI.enabled = killerDebug != victimDebug;
            if (GUILayout.Button(killerDebug.Name + " kills " + victimDebug.Name))
            {
                if (victimDebug.Pawn == null)
                    throw new Exception(victimDebug.Name + " does not have a pawn!");

                HealthController health = victimDebug.Pawn.HealthController;
                health.ApplyDamage(new BulletDamage(killerDebug, health.MaxHealth));
            }
            GUI.enabled = true;
        }
        GUILayout.EndVertical();
    }

    private void DrawDummyCreatorPanel()
    {
        GUILayout.BeginVertical("Dummy Creator", "window");
        if (GUILayout.Button("New Dummy with pawn"))
            playersManager.CreateDummyPlayer(true);
        if (GUILayout.Button("New Dummy without pawn"))
            playersManager.CreateDummyPlayer(false);
        GUILayout.EndVertical();
    }

    private void DrawPlayersList(bool drawDeveloperOptions)
    {
        GUILayout.BeginVertical("PlayerList (" + playersManager.PlayerCount + ")", "window");
        scrollPosition = GUILayout.BeginScrollView(scrollPosition);
        if (playersUIList.Count == 0)
        {
            GUILayout.Label("Empty");
        }
        else
        {
            int index = 0;
            foreach (PlayerUIWrapper playerUI in playersUIList)
            {
                DrawPlayerInfo(playerUI, index, drawDeveloperOptions);
                index++;
            }
        }
        GUILayout.EndScrollView();
        GUILayout.EndVertical();
    }

    private void DrawPlayerInfo(PlayerUIWrapper playerWrapper, int index, bool drawDeveloperOptions)
    {
        IPlayer player = playerWrapper.Player;
        GUILayout.BeginVertical("box");

        GUILayout.BeginHorizontal();
        GUILayout.Label(index + 1 + ": " + player.Name + " (" + player.GetType() + ")" + (player.IsLocal ? "(me) " : ""));
        if (drawDeveloperOptions)
        {
            playerWrapper.nameField = GUILayout.TextField(playerWrapper.nameField, GUILayout.Width(75));
            if (GUILayout.Button("Change", GUILayout.Width(60)))
                player.SetName(playerWrapper.nameField);
        }
        GUILayout.EndHorizontal();

        GUILayout.Label("id: " + player.ID);

        if(player is IClient)
        {
            IClient client = player as IClient;
            GUILayout.Label("IpAddress: " + client.IpAddress);
            GUILayout.Label("DeviceId: " + client.DeviceID);
        }

        GUILayout.BeginHorizontal();
        GUILayout.Label("Team: " + player.Team.ToString(), GUILayout.Width(100));
        if(drawDeveloperOptions)
            playerWrapper.TeamIndex = GUILayout.Toolbar(playerWrapper.TeamIndex, teamStrings, GUILayout.MaxWidth(230));
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        GUI.enabled = player.IsLocal;
        GUILayout.Label("kills: " + player.Stats.Kills);
        if (drawDeveloperOptions)
        {
            if (GUILayout.Button("+", GUILayout.Width(25)))
                player.Stats.Kills.Add(1);
            if (GUILayout.Button("-", GUILayout.Width(25)))
                player.Stats.Kills.Add(-1);
        }

        GUILayout.Label("deaths: " + player.Stats.Deaths);
        if (drawDeveloperOptions)
        {
            if (GUILayout.Button("+", GUILayout.Width(25)))
                player.Stats.Deaths.Add(1);
            if (GUILayout.Button("-", GUILayout.Width(25)))
                player.Stats.Deaths.Add(-1);
        }
        GUI.enabled = true;
        GUILayout.EndHorizontal();

        if (drawDeveloperOptions && player.Pawn != null)
        {
            PlayerHealthController healthController = player.Pawn.HealthController;
            GUILayout.BeginHorizontal();
            GUILayout.Label("HP: " + healthController.CurrentHealth);
            GUILayout.Label("State: " + healthController.State);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Label("IsRespawning: " + healthController.IsRespawning);
            GUILayout.Label("CanRespawn: " + healthController.CanRespawn);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            playerWrapper.debugDamage = GUILayout.TextField(playerWrapper.debugDamage);
            GUI.enabled = healthController.State == PlayerState.Alive;
            if (GUILayout.Button("Apply Damage"))
            {
                if (float.TryParse(playerWrapper.debugDamage, out float damage))
                    healthController.ApplyDamage(new Damage(damage));
            }

            if (GUILayout.Button("Kill"))
                healthController.SetHealth(0);
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUI.enabled = healthController.State == PlayerState.Dead;
            if (GUILayout.Button("Respawn"))
                healthController.Respawn();

            playerWrapper.respawnTimer = GUILayout.TextField(playerWrapper.respawnTimer);
            if (GUILayout.Button("Respawn Delayed"))
            {
                if (float.TryParse(playerWrapper.debugDamage, out float duration))
                {
                    healthController.RespawnDelayed(duration);
                }
            }
            GUI.enabled = true;
            GUILayout.EndHorizontal();
        }

        if (player is DummyPlayer)
        {
            if (GUILayout.Button("Remove"))
                playersManager.RemovePlayer(player);
        }

        GUILayout.EndVertical();
    }

    private void OnPlayerLeftEvent(PlayerLeftEvent obj)
    {
        BuildPlayerUIlist();
    }

    private void OnPlayerJoinEvent(PlayerJoinEvent @event)
    {
        BuildPlayerUIlist();
    }

    private void BuildPlayerUIlist()
    {
        playersUIList = new List<PlayerUIWrapper>();
        playerNames = new string[playersManager.AllPlayers.Count];
        for (int i = 0; i < playersManager.AllPlayers.Count; i++)
        {
            IPlayer player = playersManager.AllPlayers[i];
            playersUIList.Add(new PlayerUIWrapper(player));
            playerNames[i] = player.Name;
        }
    }
}
