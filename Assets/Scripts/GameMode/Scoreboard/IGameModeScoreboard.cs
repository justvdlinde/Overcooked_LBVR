using System;

public interface IGameModeScoreboard : IDisposable
{
    void Reset();
    string ToString();
}
