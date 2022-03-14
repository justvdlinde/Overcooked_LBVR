public class BulletDamage : IDamage
{
    public float Amount { get; set; }
    public DamageType DamageType => DamageType.EnemyPlayer;

    public readonly IPlayer Shooter;
    public readonly BodyPart HitBodyPart;
    // weapon

    public BulletDamage(IPlayer shooter, float amount)
    {
        Shooter = shooter;
        Amount = amount;
        HitBodyPart = BodyPart.Undefined;
    }

    public BulletDamage(IPlayer shooter, BodyPart bodyPart, float amount) 
    {
        Shooter = shooter;
        Amount = amount;
        HitBodyPart = bodyPart;
    }

    public bool IsHeadshot()
    {
        return HitBodyPart == BodyPart.Head;
    }
}
