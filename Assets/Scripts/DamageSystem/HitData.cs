using UnityEngine;

/// <summary>
/// Information on a <see cref="IShootable"/> object hit.
/// </summary>
public class HitData
{
    public IShootable ObjectHit { get; private set; }
    public BulletDamage Damage { get; private set; }
	public Vector3 Point { get; private set; }
	public Vector3 Direction { get; private set; }

	public HitData(IShootable objectHit, BulletDamage damage)
	{
		ObjectHit = objectHit;
		Damage = damage;
		Point = Vector3.zero;
		Direction = Vector3.down;
	}

	public HitData(IShootable objectHit, BulletDamage damage, Vector3 point, Vector3 direction)
	{
		ObjectHit = objectHit;
		Damage = damage;
		Point = point;
		Direction = direction;
	}
}
