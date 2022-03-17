/// <summary>
/// Information about a player's death
/// </summary>
public class PlayerDeathInfo 
{
    public readonly IPlayer DeadPlayer;
    public readonly IDamage Damage;

    public PlayerDeathInfo(IPlayer deadPlayer, IDamage damage)
    {
        DeadPlayer = deadPlayer;
        Damage = damage;
    }
}
