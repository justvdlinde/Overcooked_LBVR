using System;
using System.Collections.Generic;

/// <summary>
/// Helper class for gamemode specific things
/// </summary>
public static class GameModeHelper
{
    private static readonly Dictionary<GameModeEnum, Type> GameModeTypePairs = new Dictionary<GameModeEnum, Type>()
    {
        { GameModeEnum.Story, typeof(StoryMode) },
    };

    public static Type GetGameModeType(GameModeEnum gamemode)
    {
        return GameModeTypePairs[gamemode];
    }
    
    private static readonly Dictionary<GameModeEnum, string> GameModePrefabNames = new Dictionary<GameModeEnum, string>()
    {
        { GameModeEnum.Story, StoryMode.PrefabName },
    };

    public static string GetGameModePrefabName(GameModeEnum gamemode)
    {
        return GameModePrefabNames[gamemode];
    }
}