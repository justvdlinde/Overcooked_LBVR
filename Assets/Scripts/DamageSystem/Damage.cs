public class Damage : IDamage
{
    public float Amount { get; set; }
    public DamageType DamageType => DamageType.Unknown;

    public Damage(float amount)
    {
        Amount = amount;
    }
}
