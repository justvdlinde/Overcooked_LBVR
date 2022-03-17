using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;

public static class PhotonExtensions
{
    public static void SetCustomProperty(this Room room, string propName, object value)
    {
        room.SetCustomProperties(GetHashtable(propName, value));
    }

    public static void SetCustomProperty(this PhotonNetworkedPlayer player, string propName, object value)
    {
        player.SetCustomProperties(GetHashtable(propName, value));
    }

    public static void SetCustomProperties(this Room room, Dictionary<string, object> properties)
    {      
        room.SetCustomProperties(GetHashtable(properties));
    }

    public static void SetCustomProperties(this PhotonNetworkedPlayer player, Dictionary<string, object> properties)
    {
        player.SetCustomProperties(GetHashtable(properties));
    }

    public static bool GetCustomProperty(this PhotonNetworkedPlayer player, string propertyName, out object value)
    {
        return player.CustomProperties.TryGetValue(propertyName, out value);
    }

    public static bool GetCustomProperty<T>(this PhotonNetworkedPlayer player, string propertyName, out T value)
    {
        bool hasProperty = player.CustomProperties.TryGetValue(propertyName, out object v);
        value = (T)v;
        return hasProperty;
    }

    private static Hashtable GetHashtable(string propName, object value)
    {
        Hashtable prop = new Hashtable
        {
            { propName, value }
        };
        return prop;
    }

    private static Hashtable GetHashtable(Dictionary<string, object> properties)
    {
        Hashtable prop = new Hashtable();
        foreach (var kvp in properties)
        {
            prop.Add(kvp.Key, kvp.Value);
        }

        return prop;
    }

    /// <summary>
    /// Manually setup a <see cref="PhotonView"/> to local player
    /// </summary>
    /// <param name="photonView"></param>
    public static void Setup(this PhotonView photonView)
    {
        PhotonNetwork.LocalCleanPhotonView(photonView);
        photonView.ViewID = PhotonNetwork.AllocateViewID(PhotonNetwork.LocalPlayer.ActorNumber);
        PhotonNetwork.RegisterPhotonView(photonView);
        photonView.TransferOwnership(PhotonNetwork.LocalPlayer);
    }

    /// <summary>
    /// Manually setup a <see cref="PhotonView"/> to remote player
    /// </summary>
    /// <param name="photonView"></param>
    public static void Setup(this PhotonView photonView, PhotonView remotePlayerViewData)
    {
        PhotonNetwork.LocalCleanPhotonView(photonView);
        photonView.ViewID = PhotonNetwork.AllocateViewID(remotePlayerViewData.Owner.ActorNumber);
        PhotonNetwork.RegisterPhotonView(photonView);
        photonView.TransferOwnership(remotePlayerViewData.Owner);
    }

    /// <summary>
    /// Manually setup a <see cref="PhotonView"/> to remote player and manual viewID
    /// </summary>
    /// <param name="photonView"></param>
    public static void Setup(this PhotonView photonView, PhotonNetworkedPlayer owner, int viewID)
    {
        PhotonNetwork.LocalCleanPhotonView(photonView);
        photonView.ViewID = viewID;
        PhotonNetwork.RegisterPhotonView(photonView);
        photonView.TransferOwnership(owner);
    }
}
