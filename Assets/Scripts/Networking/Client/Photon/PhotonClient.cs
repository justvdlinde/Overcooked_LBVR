using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System;
using UnityEngine;

public abstract class PhotonClient : IClient, IDisposable
{
    public PhotonNetworkedPlayer NetworkClient { get; protected set; }
    public string UserId => NetworkClient.UserId;
    public string IpAddress { get; protected set; }
    public string DeviceID { get; protected set; }
    public bool IsLocal => NetworkClient != null && NetworkClient.IsLocal;

    public PhotonClient(PhotonNetworkedPlayer networkClient, string ipAddress = "")
    {
        NetworkClient = networkClient;
        PhotonNetworkService.PlayerPropertiesChangedEvent += OnPlayerPropertiesChangedEvent;

        if (IsLocal)
        {
            IpAddress = ipAddress;
            NetworkClient.SetCustomProperty(PlayerPropertiesPhoton.IP_ADDRESS, IpAddress);
            DeviceID = SystemInfo.deviceUniqueIdentifier;
            NetworkClient.SetCustomProperty(PlayerPropertiesPhoton.DEVICE_ID, DeviceID);
        }
        else
        {
            if (NetworkClient.GetCustomProperty(PlayerPropertiesPhoton.IP_ADDRESS, out string value))
                IpAddress = value;
            if (NetworkClient.GetCustomProperty(PlayerPropertiesPhoton.DEVICE_ID, out value))
                DeviceID = value;
        }
    }

    protected virtual void OnPlayerPropertiesChangedEvent(PhotonNetworkedPlayer targetPlayer, Hashtable properties)
    {
        if (targetPlayer != NetworkClient)
            return;
    }

    public virtual void Dispose()
    {
        PhotonNetworkService.PlayerPropertiesChangedEvent -= OnPlayerPropertiesChangedEvent;
    }
}
