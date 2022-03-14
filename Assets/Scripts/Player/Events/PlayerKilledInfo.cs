public class PlayerKilledInfo : PlayerDeathInfo
{
    public readonly IPlayer Killer;

    public PlayerKilledInfo(IPlayer deadPlayer, IDamage damage, IPlayer killer) : base(deadPlayer, damage) 
    {
        Killer = killer;
    }
}
