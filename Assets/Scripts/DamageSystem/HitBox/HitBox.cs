using System;
using UnityEngine;
using UnityEngine.Events;
using Utils.Core;

/// <summary>
/// 'Generic' hitbox component, place on any gameobject and subscribe to HitEvent.
/// </summary>
[RequireComponent(typeof(Collider))]
public class HitBox : MonoBehaviour, IShootable
{
    public Action<HitData> HitEvent { get; set; }
    [SerializeField] private UnityEvent onHit = null;
    public bool StopOnHit => stopOnHit;

    [Tooltip("Should the bullet stop on impact? Disable this if for example the bullet should go through it like a window.")]
    [SerializeField] private bool stopOnHit = true;
    [SerializeField] private Collider[] colliders = null;

    private void OnValidate()
    {
        colliders = GetComponents<Collider>();
    }

    public virtual void Hit(BulletDamage damage)
    {
        HitEvent?.Invoke(new HitData(this, damage));
        onHit.Invoke();
    }

    public virtual void Hit(BulletDamage damage, Vector3 hitPoint, Vector3 hitDirection)
    {
        HitEvent?.Invoke(new HitData(this, damage, hitPoint, hitDirection));
        onHit.Invoke();
    }

    public void EnableColliders(bool enable)
    {
        foreach (Collider collider in colliders)
            collider.enabled = enable;
    }

    protected virtual void OnDrawGizmos()
    {
        if (!enabled)
            return;

        foreach (BoxCollider boxCollider in GetComponentsInChildren<BoxCollider>())
        {
            if(boxCollider.enabled)
                GizmosUtility.DrawWireBox(transform, transform.position + boxCollider.center, boxCollider.size, Color.red);
        }
    }
}
