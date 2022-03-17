using System;

/// <summary>
/// Statistics manager for Photon Players, manages <see cref="PhotonPlayerStatHandler"/>s foreach statistic that needs to be synchronized
/// </summary>
public class PhotonPlayerStatisticsManager : IDisposable
{
    private PhotonPlayerStatHandler killsHandler;
    private PhotonPlayerStatHandler deathsHandler;

    public PhotonPlayerStatisticsManager(PhotonPlayer player, PlayerStatistics stats)
    {
        killsHandler = new PhotonPlayerStatHandler(player, stats.Kills, PlayerStatsPropertiesPhoton.KILLS);
        deathsHandler = new PhotonPlayerStatHandler(player, stats.Deaths, PlayerStatsPropertiesPhoton.DEATHS);
    }

    public void Dispose()
    {
        killsHandler.Dispose();
        deathsHandler.Dispose();
    }
}
