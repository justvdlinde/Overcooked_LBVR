using System;

/// <summary>
/// Container script for all player related statistics such as kills or deaths
/// </summary>
public class PlayerStatistics : IDisposable
{
    public PlayerStatInt Kills { get; protected set; }
    public PlayerStatInt Deaths { get; protected set; }

    public PlayerStatistics()
    {
        Kills = new PlayerStatInt();
        Deaths = new PlayerStatInt();
    }

    public void Reset()
    {
        Kills.Reset();
        Deaths.Reset();
    }

    public virtual void Dispose() 
    {
        Kills.Dispose();
        Deaths.Dispose();
    }
}
