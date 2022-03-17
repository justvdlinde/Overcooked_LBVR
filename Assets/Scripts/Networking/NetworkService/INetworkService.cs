using System;
using System.Collections.Generic;
using System.Net;
using Utils.Core.Services;

public enum NetworkStatus
{
    /// <summary>
    /// Not connected to a network
    /// </summary>
    Offline,
    /// <summary>
    /// Attempting to connect to a network
    /// </summary>
    Connecting,
    /// <summary>
    /// Joining a lobby or room
    /// </summary>
    Joining,
    /// <summary>
    /// Connected to a lobby or room
    /// </summary>
    Connected
};

public interface INetworkService : IService
{
    public NetworkStatus Status { get; }
    public IClient LocalClient { get; }
    public List<IClient> AllClients { get; }
    public int ClientCount { get; }

    public Action ConnectEvent { get; set; }
    public Action<IDisconnectInfo> DisconnectEvent { get; set; }
    public Action<IClientConnectionData> ClientJoinedEvent { get; set; }
    public Action<IClientDisconnectData> ClientLeftEvent { get; set; }

    public void Connect(NetworkConfig config);
    public void Disconnect();
    public void Disconnect(IDisconnectInfo info);

    public void AddClient(IClient client);
    public void RemoveClient(IClient client);
}
