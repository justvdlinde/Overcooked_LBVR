public class FallDamage : IDamage
{
    public float Amount { get; set; }
    public DamageType DamageType => DamageType.FallDamage;
    // fall height

    public FallDamage(float amount) 
    {
        Amount = amount;
    }
}

