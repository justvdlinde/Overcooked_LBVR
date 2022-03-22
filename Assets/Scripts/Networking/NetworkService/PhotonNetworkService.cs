using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils.Core.Events;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public enum ConnectionType
{
    /// <summary>
    /// Connects to a 'worldwide' cloud room, useful for testing
    /// </summary>
    Cloud,
    /// <summary>
    /// Searches for a room on the network and attempts to join it via cloud
    /// </summary>
    CloudLocalRoom,
    /// <summary>
    /// Searches for a room on the network and attempts to join it locally
    /// </summary>
    Selfhosted
};

public abstract class PhotonNetworkService : MonoBehaviourPunCallbacks, INetworkService, IOnEventCallback
{
    public delegate void PlayerPropertiesChangeDelegate(PhotonNetworkedPlayer targetPlayer, Hashtable properties);

    public NetworkStatus Status { get; private set; } = NetworkStatus.Offline;
    public Action ConnectEvent { get; set; }
    public Action<IDisconnectInfo> DisconnectEvent { get; set; }
    public Action<IClientConnectionData> ClientJoinedEvent { get; set; }
    public Action<IClientDisconnectData> ClientLeftEvent { get; set; }

    public IClient LocalClient { get; protected set; }
    public List<IClient> AllClients { get; protected set; }
    public int ClientCount => AllClients.Count;
    protected Dictionary<string, IClient> clientIdTable;

    private const float CLIENT_CONNECTION_ATTEMPT_DURATION = 1;

    public const string GAME_CODE = "CvsR";
    public ConnectionType ConnectionType { get; private set; }

    public Action<short> JoinRoomFailedEvent;

    public static event PlayerPropertiesChangeDelegate PlayerPropertiesChangedEvent;
    public static Action<EventData> PhotonEventReceivedEvent;
    public static Action<Hashtable> RoomPropertiesChangedEvent;
    public static Action<PhotonNetworkedPlayer> MasterClientSwitchedEvent;

    protected GlobalEventDispatcher globalEventDispatcher;
    protected UDPServerDiscoveryService serverDiscoveryService;
    protected PopupService popupService;
    protected string roomName = "";

    public void InjectDependencies(GlobalEventDispatcher globalEventDispatcher, UDPServerDiscoveryService serverDiscoveryService, PopupService popupService)
    {
        this.globalEventDispatcher = globalEventDispatcher;
        this.serverDiscoveryService = serverDiscoveryService;
        this.popupService = popupService;
    }

    private void Awake()
    {
        DontDestroyOnLoad(this);
        PhotonNetwork.KeepAliveInBackground = 5;
        PhotonNetwork.UseRpcMonoBehaviourCache = true;
        PhotonNetwork.AutomaticallySyncScene = false;
        PhotonNetwork.SerializationRate = 20;
        PhotonNetwork.SendRate = 40;

        AllClients = new List<IClient>();
        clientIdTable = new Dictionary<string, IClient>();
    }

    public virtual void Connect(NetworkConfig config)
    {
        Status = NetworkStatus.Connecting;

        switch (config.connectionType)
        {
            case ConnectionType.Cloud:
                ConnectToCloud(config.roomName);
                break;
            case ConnectionType.CloudLocalRoom:
                ConnectToLocalCloudRoom(config);
                break;
            case ConnectionType.Selfhosted:
                ConnectToSelfHosted(config);
                break;
            default:
                throw new NotImplementedException("No corresponding connection type for " + config.connectionType);
        }
    }

    public virtual void Disconnect()
    {
        PhotonNetwork.Disconnect();
    }

    public virtual void Disconnect(IDisconnectInfo info)
    {
        PhotonNetwork.Disconnect((DisconnectCause)info.ErrorCode);
    }

    #region Functions
    protected virtual void ConnectToRoom()
    {
        if (!PhotonNetwork.IsConnected)
            return;

        Status = NetworkStatus.Joining;

        RoomOptions options = new RoomOptions
        {
            PublishUserId = true,
            //CleanupCacheOnLeave = false
        };
        PhotonNetwork.JoinOrCreateRoom(roomName, options, new TypedLobby(roomName, LobbyType.Default));
    }

    protected virtual void ConnectToCloud(string roomName)
    {
        if (Application.isPlaying && Application.internetReachability == NetworkReachability.NotReachable)
        {
            popupService.SpawnPopup().Setup("No Internet", "Make sure you are connected with the internet. ", new InteractablePopup.ButtonData("Retry", () => ConnectToCloud(roomName)));
            Status = NetworkStatus.Offline;
            return;
        }

        ConnectionType = ConnectionType.Cloud;
        this.roomName = roomName;
        PhotonNetwork.ConnectToCloud();
        //PhotonNetwork.GameVersion = config.GameVersion;
    }

    protected virtual void ConnectToLocalCloudRoom(NetworkConfig config)
    {
        if (GamePlatform.GameType == ClientType.Operator)
            ConnectToCloudAndBroadcastRoomName(config);
        else
            ListenForBroadcastAndConnect(config);
    }

    protected abstract void ConnectToSelfHosted(NetworkConfig config);

    protected void ListenForBroadcastAndConnect(NetworkConfig config)
    {
        ListenToBroadcastAndConnect(config, 0);
    }

    protected void ListenToBroadcastAndConnect(NetworkConfig config, float attempts)
    {
        float maxAttempts = config.clientConnectionAttemptLimit;

        Debug.Log("attempting to connect, attempt nr: " + attempts + "/" + maxAttempts);
        if (attempts < maxAttempts)
        {
            serverDiscoveryService.SearchForServer
            (
                GAME_CODE,
                // on success:
                (GameServer server) =>
                {
                    if (server.Message == "" || server.Message == string.Empty)
                    {
                        Debug.Log("Found gameserver for selfhosted");
                        ConnectToServerViaIpAddress(config, server.IpAddress);
                    }
                    else
                    {
                        Debug.Log("Found gameserver for localCloud");
                        ConnectToCloud(server.Message);
                    }
                },
                // on fail:
                (UDPServerDiscoveryService.ServerNotFoundCause cause) =>
                {
                    if (cause == UDPServerDiscoveryService.ServerNotFoundCause.NotFound || cause == UDPServerDiscoveryService.ServerNotFoundCause.Exception)
                    {
                        StartCoroutine(Wait(CLIENT_CONNECTION_ATTEMPT_DURATION, () => ListenToBroadcastAndConnect(config, ++attempts)));
                        return;
                    }
                }
            );
        }
        else
        {
            globalEventDispatcher.Invoke(new ConnectionFailedEvent(new PhotonDisconnectInfo(DisconnectCause.ServerUDPSearchTimeout)));
        }
    }

    protected virtual void ConnectToCloudAndBroadcastRoomName(NetworkConfig config)
    {
        ConnectEvent += OnConnectSuccess;
        DisconnectEvent += OnConnectFail;
        ConnectToCloud(MacAddressHelper.GetMacAddress());

        void OnConnectSuccess()
        {
            serverDiscoveryService.StartListeningForClients(GAME_CODE, config.roomName);

            ConnectEvent -= OnConnectSuccess;
            DisconnectEvent -= OnConnectFail;
        }

        void OnConnectFail(IDisconnectInfo info)
        {
            ConnectEvent -= OnConnectSuccess;
            DisconnectEvent -= OnConnectFail;
        }
    }

    protected virtual void ConnectToServerViaIpAddress(NetworkConfig config, string ipAddress)
    {
        ConnectionType = ConnectionType.Selfhosted;
        ServerSettings serverSettings = PhotonNetwork.PhotonServerSettings;
        PhotonNetwork.ConnectToMaster(ipAddress, serverSettings.AppSettings.Port, serverSettings.AppSettings.AppIdRealtime);
    }

    protected virtual void StartSearchForLocalServer(NetworkConfig config)
    {
        ConnectionType = ConnectionType.Selfhosted;
        SearchForLocalServer(config, 0);
    }

    protected virtual void SearchForLocalServer(NetworkConfig config, float attemptNr)
    {
        float maxAttempts = config.clientConnectionAttemptLimit;

        Debug.Log("attempting to connect, attempt nr: " + attemptNr + "/" + maxAttempts);
        if (attemptNr < maxAttempts)
        {
            serverDiscoveryService.SearchForServer
            (
                GAME_CODE,
                // on success:
                (GameServer server) =>
                {
                    Debug.Log("Server found on " + server.IpAddress + ", attempting to connect to server");
                    ConnectToServerViaIpAddress(config, server.IpAddress);
                },
                // on fail:
                (UDPServerDiscoveryService.ServerNotFoundCause cause) =>
                {
                    if (cause == UDPServerDiscoveryService.ServerNotFoundCause.CodeMismatch)
                    {
                        Debug.Log("Server found but code was mismatch");
                        globalEventDispatcher.Invoke(new ConnectionFailedEvent(new PhotonDisconnectInfo(DisconnectCause.GameVersionMismatch)));

                        return;
                    }
                    else
                    {
                        StartCoroutine(Wait(CLIENT_CONNECTION_ATTEMPT_DURATION, () => SearchForLocalServer(config, ++attemptNr)));
                    }
                }
            );
        }
        else
        {
            globalEventDispatcher.Invoke(new ConnectionFailedEvent(new PhotonDisconnectInfo(DisconnectCause.ServerUDPSearchTimeout)));
        }
    }

    protected IEnumerator Wait(float duration, Action onDone)
    {
        yield return new WaitForSeconds(duration);
        onDone?.Invoke();
    }

    protected bool MatchesRoomVersion(string roomVersion)
    {
        return Application.version == roomVersion;
    }

    public void AddClient(IClient client)
    {
        if (client == null)
            throw new NullReferenceException("Client is null");

        AllClients.Add(client);
        clientIdTable.Add(client.UserId, client);

        if (client.IsLocal)
            LocalClient = client;

        IClientConnectionData data = new PhotonClientConnectionData(client as PhotonClient);
        ClientJoinedEvent?.Invoke(data);
        globalEventDispatcher.Invoke(new ClientJoinEvent(data));
    }

    public void RemoveClient(IClient client)
    {
        AllClients.Remove(client);
        clientIdTable.Remove(client.UserId);

        if (client == LocalClient)
            LocalClient = null;

        IClientDisconnectData data = new PhotonClientDisconnectData(client as PhotonClient);
        ClientLeftEvent?.Invoke(data);
        globalEventDispatcher.Invoke(new ClientLeftEvent(data));

        Debug.LogFormat("Client({0}) ID: {1} left game", client.GetType(), client.UserId);
    }

    protected void RemoveAllNonLocalClients()
    {
        for (int i = AllClients.Count - 1; i >= 0; i--)
        {
            IClient client = AllClients[i];
            if (!client.IsLocal)
            {
                RemoveClient(client);
            }
        }
    }

    /// <summary>
    /// Returns client using NetworkClient.UserID
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public IClient GetClientByID(string id)
    {
        if (clientIdTable.TryGetValue(id, out IClient client))
            return client;
        else
            return null;
    }
    
    private IClient CreateClient(PhotonNetworkedPlayer photonPlayer, ClientType clientType)
    {
        IClient client;
        switch (clientType)
        {
            case ClientType.Player:
                PlayerFactory playerFactory = new PlayerFactory(photonPlayer);
                client = playerFactory.Construct() as IClient;
                break;
            case ClientType.Operator:
                OperatorFactory operatorFactory = new OperatorFactory(photonPlayer);
                client = operatorFactory.Construct();
                break;
            case ClientType.Spectator:
                SpectatorFactory spectatorFactory = new SpectatorFactory(photonPlayer);
                client = spectatorFactory.Construct();
                break;
            default:
                throw new NotImplementedException("No corresponding factory for " + clientType);
        }

        AddClient(client);
        return client;
    }

    private IClient CreateClient(PhotonNetworkedPlayer photonPlayer)
    {
        photonPlayer.GetCustomProperty(PlayerPropertiesPhoton.CLIENT_TYPE, out object value);
        if (value == null)
        {
            if (PhotonNetwork.IsMasterClient)
                PhotonNetwork.CloseConnection(photonPlayer);
            throw new Exception("Client without ClientType joined, this is not allowed, client will be kicked.");
        }
        return CreateClient(photonPlayer, (ClientType)value);
    }

    /// <summary>
    /// When the localplayer is created it's id is -1 (offline), this changes when going online, so the clientIdTable has to be updated accordingly
    /// </summary>
    /// <param name="localClient"></param>
    private void UpdateClientIdTable()
    {
        if (LocalClient == null)
            return;

        foreach (var kvp in clientIdTable)
        {
            if (kvp.Value == LocalClient)
            {
                clientIdTable.Remove(kvp.Key);
                break;
            }
        }

        clientIdTable.Add(LocalClient.UserId, LocalClient);
    }

    private void CreateConnectedClients()
    {
        for (int i = 0; i < PhotonNetwork.PlayerList.Length; i++)
        {
            PhotonNetworkedPlayer photonPlayer = PhotonNetwork.PlayerList[i];
            if (photonPlayer.IsLocal)
                continue;
            CreateClient(photonPlayer);
        }
    }
    #endregion

    #region Events
    public override void OnConnectedToMaster()
    {
        base.OnConnectedToMaster();
        DebugPrint.Log("OnConnectedToMasterEvent");
        ConnectToRoom();
    }

    public override void OnCreatedRoom()
    {
        base.OnCreatedRoom();
        DebugPrint.Log("OnCreatedRoomEvent");
    }

    public override void OnDisconnected(DisconnectCause cause)
    {
        Debug.Log("OnDisconnected");

        base.OnDisconnected(cause);

        if (cause == DisconnectCause.DisconnectByClientLogic)
            Debug.Log("OnDisconnected, error: " + cause);
        else
            Debug.LogWarning("OnDisconnected, error: " + cause);

        RemoveAllNonLocalClients();
        Status = NetworkStatus.Offline;
        IDisconnectInfo info = new PhotonDisconnectInfo(cause);
        DisconnectEvent?.Invoke(info);
        globalEventDispatcher.Invoke(new ConnectionFailedEvent(info));

        if (GamePlatform.GameType == ClientType.Operator)
        {
            if (serverDiscoveryService.IsListeningForClients)
                serverDiscoveryService.StopListeningForClients();
        }

        PhotonNetwork.OfflineMode = true;
    }

    public override void OnJoinedRoom()
    {
        base.OnJoinedRoom();

        if (PhotonNetwork.OfflineMode)
            return;
        Status = NetworkStatus.Connected;

        DebugPrint.Log("OnJoinedRoomEvent, is masterClient " + PhotonNetwork.IsMasterClient + " player count: " + PhotonNetwork.CurrentRoom?.PlayerCount);

        CreateConnectedClients();
        UpdateClientIdTable();

        globalEventDispatcher.Invoke(new ConnectionSuccessEvent(PhotonNetwork.CurrentRoom));
        ConnectEvent?.Invoke();

        if (GamePlatform.GameType == ClientType.Operator)
        {
            if (ConnectionType == ConnectionType.CloudLocalRoom)
                serverDiscoveryService.StartListeningForClients(GAME_CODE, roomName);
        }
    }

    public override void OnJoinRoomFailed(short returnCode, string message)
    {
        base.OnJoinRoomFailed(returnCode, message);
        DebugPrint.Log("OnJoinRoomFailed, code: " + returnCode + " error: " + message);
        JoinRoomFailedEvent?.Invoke(returnCode);
    }

    public override void OnCreateRoomFailed(short returnCode, string message)
    {
        base.OnCreateRoomFailed(returnCode, message);
        DebugPrint.LogError("OnCreateRoomFailed, code: " + returnCode + " error: " + message);
    }

    public override void OnPlayerEnteredRoom(PhotonNetworkedPlayer player)
    {
        base.OnPlayerEnteredRoom(player);

        DebugPrint.Log("OnPlayerEnteredRoom: " + player);

        CreateClient(player);
    }

    public override void OnPlayerLeftRoom(PhotonNetworkedPlayer player)
    {
        base.OnPlayerLeftRoom(player);

        if (PhotonNetwork.IsMasterClient)
        {
            PhotonNetwork.RemoveRPCs(player);
            PhotonNetwork.DestroyPlayerObjects(player);
            PhotonNetwork.OpCleanActorRpcBuffer(player.ActorNumber);
        }

        DebugPrint.Log("OnPlayerLeftRoom: " + player);

        PhotonClient client = GetClientByID(player.UserId) as PhotonClient;
        RemoveClient(client);
    }

    public override void OnRoomPropertiesUpdate(Hashtable properties)
    {
        base.OnRoomPropertiesUpdate(properties);

        DebugPrint.Log("OnRoomPropertiesChangedEvent");
        RoomPropertiesChangedEvent?.Invoke(properties);
    }

    public override void OnPlayerPropertiesUpdate(PhotonNetworkedPlayer targetPlayer, Hashtable properties)
    {
        base.OnPlayerPropertiesUpdate(targetPlayer, properties);

        DebugPrint.Log("OnPlayerPropertiesChangedEvent");
        PlayerPropertiesChangedEvent?.Invoke(targetPlayer, properties);
    }

    public void OnEvent(EventData eventData)
    {
        PhotonEventReceivedEvent?.Invoke(eventData);
    }

    public override void OnMasterClientSwitched(PhotonNetworkedPlayer newMasterClient)
    {
        base.OnMasterClientSwitched(newMasterClient);
        MasterClientSwitchedEvent?.Invoke(newMasterClient);
    }

    private void OnApplicationQuit()
    {
        if (serverDiscoveryService.IsListeningForClients)
            serverDiscoveryService.StopListeningForClients();
    }
    #endregion

    #region Debugging
    private static class DebugPrint
    {
        private const string DEBUG_NETWORKING_DIRECTIVE = "DEBUG_NETWORKING";

        [System.Diagnostics.Conditional(DEBUG_NETWORKING_DIRECTIVE)]
        public static void Log(string message)
        {
            Debug.Log(message);
        }

        [System.Diagnostics.Conditional(DEBUG_NETWORKING_DIRECTIVE)]
        public static void LogWarning(string message)
        {
            Debug.LogWarning(message);
        }

        [System.Diagnostics.Conditional(DEBUG_NETWORKING_DIRECTIVE)]
        public static void LogError(string message)
        {
            Debug.LogWarning(message);
        }
    }
#endregion
}

