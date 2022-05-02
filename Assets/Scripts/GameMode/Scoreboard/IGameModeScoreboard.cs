using System;

public interface IGameModeScoreboard : IDisposable
{
    int OrdersCount { get;}
    float TotalPoints { get; }
    float MaxAchievablePoints { get; }
    int DeliveredOrdersCount { get; }
    int TimerExceededOrdersCount { get; }
    int PerfectOrdersCount { get; }

    void Reset();
    string ToString();
}
