using UnityEngine;

public interface IShootable
{
    /// <summary>
    /// Should the bullet stop and destroy itself on hit?
    /// </summary>
    bool StopOnHit { get; }
    void Hit(BulletDamage damage);
    void Hit(BulletDamage damage, Vector3 hitPoint, Vector3 hitDirection);
}