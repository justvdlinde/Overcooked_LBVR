using UnityEngine;

public enum BuildPlatform
{
    Android,
    Windows
}

public enum AndroidDevice
{
    Oculus,
    Pico
}

public static class GamePlatform
{
    public static ClientType GameType { get; private set; } = ClientType.Player;
    public static BuildPlatform Platform { get; private set; } 

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
    public static void OnGameInit()
    {
#if GAME_CLIENT
        GameType = ClientType.Player;
#elif GAME_OPERATOR
        GameType = ClientType.Operator;
#elif GAME_SPECTATOR
        GameType = ClientType.Spectator;
#endif

#if UNITY_STANDALONE
        Platform = BuildPlatform.Windows;
#elif UNITY_ANDROID
        Platform = BuildPlatform.Android;
#endif

    }
}
