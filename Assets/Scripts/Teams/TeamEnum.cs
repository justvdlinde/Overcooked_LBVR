/// <summary>
/// Enum defining the teams available. First team should be neutral (it's the default value any field of this enum gets).
/// </summary>
public enum Team : byte
{
    /// <summary>
    /// No Team, default setting when joining
    /// </summary>
    None,
    /// <summary>
    /// Specific team when playing a gamemode without teams, such as FreeForAll
    /// </summary>
    FreeForAll,
    /// <summary>
    /// Team Cops
    /// </summary>
    Team1,
    /// <summary>
    /// Team Robbers
    /// </summary>
    Team2
};
