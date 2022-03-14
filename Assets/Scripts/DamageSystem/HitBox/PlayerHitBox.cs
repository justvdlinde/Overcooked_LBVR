using UnityEngine;

/// <summary>
/// Player specific hitbox, contains information of <see cref="BodyPartType"/>
/// </summary>
public class PlayerHitBox : HitBox
{
    public BodyPart BodyPartType => hitBoxType;
    public float DamageMultiplier => damageMultiplier;

    [SerializeField] private BodyPart hitBoxType = BodyPart.Body;
    [SerializeField] private ParticleSystem hitParticlePrefab = null;
    [SerializeField] private float damageMultiplier = 1f;

    public override void Hit(BulletDamage damage, Vector3 hitPoint, Vector3 hitDirection)
    {
        base.Hit(damage, hitPoint, hitDirection);
        CreateHitEffect(hitPoint, hitDirection);
    }

    private void CreateHitEffect(Vector3 point, Vector3 direction)
    {
        if (hitParticlePrefab != null)
        {
            // TODO: instantiate in awake or use prefab and use particle.play
            ParticleSystem particle = Instantiate(hitParticlePrefab, point, Quaternion.LookRotation(direction));
            Destroy(particle.gameObject, particle.main.duration);
        }
    }
}
