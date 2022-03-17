using Photon.Pun;
using System;
using UnityEngine;
using Utils.Core.Attributes;

public class PlayerHitBoxController : MonoBehaviourPun
{
    public bool HitBoxesAreEnabled { get; protected set; }
    public IPlayer Player { get; protected set; }

    public Action<HitData> hitEvent;

    [SerializeField] private HealthController healthController = null;
    [SerializeField] private PlayerHitBox[] hitBoxes = null;

    public void InjectDependencies(IPlayer player)
    {
        Player = player;
    }

    protected virtual void Awake()
    {
        foreach (HitBox hitBox in hitBoxes)
        {
            hitBox.HitEvent += OnHitBoxHit;
        }

        EnableHitBoxes(true);
    }

    protected virtual void Start()
    {
        EnableHitBoxes(true);
    }

    public virtual void EnableHitBoxes(bool enable)
    {
        foreach (PlayerHitBox hitBox in hitBoxes)
            hitBox.EnableColliders(enable);

        HitBoxesAreEnabled = enable;
    }

    protected virtual void OnHitBoxHit(HitData hitData)
    {
        if (!HitBoxesAreEnabled)
            return;

        hitEvent?.Invoke(hitData);
        if (!IsFriendlyFire(hitData.Damage))
            healthController.ApplyDamage(hitData.Damage);
    }

    protected bool IsFriendlyFire(BulletDamage damage)
    {
        return false;

        //if (Player == null)
        //    return false;
        //else
        //    return damage.Shooter.Team == Player.Team;
    }

    protected virtual void OnDestroy()
    {
        EnableHitBoxes(false);
    }

#if UNITY_EDITOR
    [Button]
    private void ToggleHitBoxes()
    {
        EnableHitBoxes(!HitBoxesAreEnabled);
    }
#endif
}
