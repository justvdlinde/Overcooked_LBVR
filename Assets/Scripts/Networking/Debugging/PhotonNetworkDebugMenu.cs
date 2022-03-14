using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Net;
using System.Net.NetworkInformation;
using UnityEngine;

public class PhotonNetworkDebugMenu : IDebugMenu
{
    private NetworkConfig config = null;

    private readonly UDPServerDiscoveryService serverDiscovery;
    private readonly PhotonNetworkService networkService;

    private PhotonPeer peer; 
    private string pingAddress;
    private Vector2 scrollPosition;
    private string[] connectionTypeStrings;
    private int networkConfigConnectionTypeIndex;
    private IPAddress ipAddress;

    public PhotonNetworkDebugMenu(INetworkService networkService, UDPServerDiscoveryService serverDiscoveryService)
    {
        this.networkService = networkService as PhotonNetworkService;
        serverDiscovery = serverDiscoveryService;

        config = ScriptableObject.CreateInstance<NetworkConfig>();
        config.roomName = "RoomName";
        config.connectionType = ConnectionType.Cloud;
        networkConfigConnectionTypeIndex = (int)config.connectionType;
        connectionTypeStrings = Enum.GetNames(typeof(ConnectionType));
        ipAddress = NetworkHelper.GetLocalIPAddress();
    }
    
    public void Open() 
    { 
        peer = PhotonNetwork.NetworkingClient.LoadBalancingPeer;
    }

    public void Close() { }

    public void OnGUI(bool drawDeveloperOptions)
    {
        GUILayout.BeginVertical("box", GUILayout.MinWidth(400));
        scrollPosition = GUILayout.BeginScrollView(scrollPosition);
        DrawInfo();
        GUILayout.Space(5);
        DrawClientsList();
        if (drawDeveloperOptions)
        {
            GUILayout.Space(5);
            DrawOptionsPanel();
            GUILayout.Space(5);
            DrawPingPanel();
            GUILayout.Space(5);
            DrawNetworkConfigPanel();
            GUILayout.Space(5);
            DrawSimulationPanel();
            GUILayout.Space(5);
            DrawServerDiscoveryPanel();
        }
        GUILayout.EndScrollView();
        GUILayout.EndVertical();
    }

    private void DrawInfo()
    {
        GUILayout.BeginVertical("Networking", "window");
        GUILayout.Label("Clientservice: " + networkService.GetType());
        GUILayout.Label("Client count (networkservice): " + networkService.ClientCount);
        GUILayout.Label("Player count (Photon): " + ((PhotonNetwork.CurrentRoom != null) ? PhotonNetwork.CurrentRoom.PlayerCount.ToString() : "-"));
        GUILayout.Label("Local client: " + (networkService.LocalClient != null ? networkService.LocalClient.ToString() : "-"));
        GUILayout.Label("Local client type: " + (networkService.LocalClient != null ? networkService.LocalClient.GetType().ToString() : "-"));
        GUILayout.Label("My Local IP: " + ipAddress);
        GUILayout.Label("NetworkService Status: " + networkService.Status);
        GUILayout.Label("Photon Network state: " + PhotonNetwork.NetworkClientState);
        GUILayout.Label("IsMasterClient: " + PhotonNetwork.IsMasterClient);
        GUILayout.Label("CurrentRoom: " + PhotonNetwork.CurrentRoom);
        GUILayout.Label("Ping: " + PhotonNetwork.GetPing());
        GUILayout.Label("ConnectionType: " + networkService.ConnectionType);
        GUILayout.Label("Offline Mode: " + PhotonNetwork.OfflineMode);
        GUILayout.EndVertical();
    }

    private void DrawClientsList()
    {
        GUILayout.BeginVertical("Clients", "window");
        int index = 0;
        if (networkService.AllClients.Count == 0)
        {
            GUILayout.Label("Empty");
        }
        else
        {
            foreach (IClient client in networkService.AllClients)
            {
                GUILayout.Label(index + 1 + ": " + client.UserId);
                index++;
            }
        }
        index = 0;
        GUILayout.EndVertical();
    }

    private void DrawOptionsPanel()
    {
        GUILayout.BeginVertical("Options", "window");
        if (GUILayout.Button("Connect (" + networkService.ConnectionType + ")"))
            networkService.Connect(config);
        if (GUILayout.Button("Disconnect"))
            networkService.Disconnect();
        if (GUILayout.Button("Disconnect Simulated"))
            networkService.Disconnect(new PhotonDisconnectInfo(DisconnectCause.Exception));
        if (GUILayout.Button("ReconnectAndRejoin"))
            PhotonNetwork.ReconnectAndRejoin();
    }

    private void DrawPingPanel()
    {
        GUILayout.BeginHorizontal();
        pingAddress = GUILayout.TextField(pingAddress);
        if (GUILayout.Button("Ping"))
            PingIPAddress(pingAddress);
        GUILayout.EndHorizontal();
        GUILayout.EndVertical();
    }

    private void DrawNetworkConfigPanel()
    {
        GUILayout.BeginVertical("Network Config", "window");
        GUILayout.BeginHorizontal();
        GUILayout.Label("Room Name");
        config.roomName = GUILayout.TextField(config.roomName);
        GUILayout.EndHorizontal();
        networkConfigConnectionTypeIndex = GUILayout.Toolbar(networkConfigConnectionTypeIndex, connectionTypeStrings);
        GUILayout.EndVertical();
    }

    private void DrawServerDiscoveryPanel()
    {
        GUILayout.BeginVertical("Server Discovery (Host)", "window");
        GUI.enabled = !serverDiscovery.IsListeningForClients;
        if (GUILayout.Button("Start listening for clients"))
            serverDiscovery.StartListeningForClients(PhotonNetworkService.GAME_CODE, "");
        GUI.enabled = serverDiscovery.IsListeningForClients;
        if (GUILayout.Button("Stop listening for clients"))
            serverDiscovery.StopListeningForClients();
        GUI.enabled = true;
        GUILayout.EndVertical();

        GUILayout.Space(5);

        GUILayout.BeginVertical("Server Discovery (Client)", "window");
        GUI.enabled = !serverDiscovery.IsListeningForClients;
        if (GUILayout.Button("Search for server"))
            serverDiscovery.SearchForServer(PhotonNetworkService.GAME_CODE);
        if (serverDiscovery.ServerFound == null)
            GUILayout.Label("No server found");
        else
            GUILayout.Label("Found server at: " + serverDiscovery.ServerFound.IpAddress);
        GUI.enabled = true;
        GUILayout.EndVertical();
    }

    private void PingIPAddress(string ip)
    {
        System.Net.NetworkInformation.Ping ping = new System.Net.NetworkInformation.Ping();
        IPAddress address = IPAddress.Parse(ip);
        PingReply pong = ping.Send(address);
        Debug.Log("pinging " + ip + ", result: " + pong.Status);
    }

    private void DrawSimulationPanel()
    {
        GUILayout.BeginVertical("Simulation", "window");
        GUILayout.Label(string.Format("Round trip time:{0,4} +/-{1,3}", peer.RoundTripTime, peer.RoundTripTimeVariance));

        bool simEnabled = peer.IsSimulationEnabled;
        bool newSimEnabled = GUILayout.Toggle(simEnabled, "Simulate");
        if (newSimEnabled != simEnabled)
        {
            peer.IsSimulationEnabled = newSimEnabled;
        }

        float inOutLag = this.peer.NetworkSimulationSettings.IncomingLag;
        GUILayout.Label("Lag: " + inOutLag);
        inOutLag = GUILayout.HorizontalSlider(inOutLag, 0, 500);

        peer.NetworkSimulationSettings.IncomingLag = (int)inOutLag;
        peer.NetworkSimulationSettings.OutgoingLag = (int)inOutLag;

        float inOutJitter = peer.NetworkSimulationSettings.IncomingJitter;
        GUILayout.Label("Jitter: " + inOutJitter);
        inOutJitter = GUILayout.HorizontalSlider(inOutJitter, 0, 100);

        peer.NetworkSimulationSettings.IncomingJitter = (int)inOutJitter;
        peer.NetworkSimulationSettings.OutgoingJitter = (int)inOutJitter;

        float loss = peer.NetworkSimulationSettings.IncomingLossPercentage;
        GUILayout.Label("Loss: " + loss);
        loss = GUILayout.HorizontalSlider(loss, 0, 10);

        peer.NetworkSimulationSettings.IncomingLossPercentage = (int)loss;
        peer.NetworkSimulationSettings.OutgoingLossPercentage = (int)loss;
        GUILayout.EndVertical();
    }
}
