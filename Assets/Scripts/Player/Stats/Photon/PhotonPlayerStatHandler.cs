using ExitGames.Client.Photon;
using System;

/// <summary>
/// For local Players, automatically updates the photon property when it's corresponding statistic is altered
/// For remote Players, sets the statistic when the corresponding photon property has changed
/// </summary>
public class PhotonPlayerStatHandler : IDisposable
{
    private readonly PhotonPlayer player;
    private readonly PlayerStatInt stat;
    private readonly string propertyName;

    public PhotonPlayerStatHandler(PhotonPlayer player, PlayerStatInt stat, string photonPropertyName)
    {
        this.player = player;
        this.stat = stat;
        propertyName = photonPropertyName;

        if (player.IsLocal)
        {
            this.stat.ChangedEvent += OnStatChangedEvent;
        }
        else
        {

            if (player.NetworkClient.CustomProperties.TryGetValue(photonPropertyName, out object value))
                stat.Set((int)value);

            player.PropertiesChangedEvent += OnPropertiesChangedEvent;
        }
    }

    protected void OnStatChangedEvent(int from, int to)
    {
        player.NetworkClient.SetCustomProperty(propertyName, to);
    }

    protected void OnPropertiesChangedEvent(Hashtable properties)
    {
        if (properties.TryGetValue(propertyName, out object value))
            stat.Set((int)value);
    }

    public void Dispose()
    {
        stat.ChangedEvent -= OnStatChangedEvent;
        player.PropertiesChangedEvent -= OnPropertiesChangedEvent;
    }
}