using System;
using System.Collections.Generic;

/// <summary>
/// Helper class for gamemode specific things
/// </summary>
public static class GameModeHelper
{
    private static readonly Dictionary<GameModeEnum, Type> GameModeTypePairs = new Dictionary<GameModeEnum, Type>()
    {
        { GameModeEnum.TeamDeathmatch, typeof(TDMGameMode) },
    };

    public static Type GetGameModeType(GameModeEnum gamemode)
    {
        return GameModeTypePairs[gamemode];
    }
}