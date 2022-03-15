using Photon.Pun;
using UnityEngine;
using Utils.Core.Events;

public class PhotonPlayerHealthController : PlayerHealthController
{
    [SerializeField] private PhotonView photonView = null;

    private PlayersManager playersManager;

    public void InjectDependencies(IPlayer player, EventDispatcher localEventDispatcher, GlobalEventDispatcher globalEventDispatcher, PlayersManager playersManager)
    {
        InjectDependencies(player, localEventDispatcher, globalEventDispatcher);
        this.playersManager = playersManager;
    }

    protected void OnValidate()
    {
        if (photonView == null)
        {
            if (gameObject.TryGetComponent(out PhotonView photonView))
                this.photonView = photonView;
        }
    }

    protected override void Start()
    {
        if (photonView.IsMine)
        {
            base.Start();
        }
        else
        {
            if (photonView.Owner.CustomProperties.TryGetValue(PlayerPropertiesPhoton.STATE, out object stateIndex))
                base.SetState((PlayerState)stateIndex);
            else
                base.SetState(PlayerState.Alive);

            if (photonView.Owner.CustomProperties.TryGetValue(PlayerPropertiesPhoton.HEALTH_POINTS, out object hp))
                base.SetHealth((float)hp);

            if(player is PhotonPlayer)
                (player as PhotonPlayer).PropertiesChangedEvent += OnPlayerPhotonPropertiesChangedEvent;
        }
    }

    protected void OnDestroy()
    {
        if (player is PhotonPlayer)
            (player as PhotonPlayer).PropertiesChangedEvent -= OnPlayerPhotonPropertiesChangedEvent;
    }

    protected void OnPlayerPhotonPropertiesChangedEvent(ExitGames.Client.Photon.Hashtable properties)
    {
        if (photonView.IsMine)
            return;

        if (properties.TryGetValue(PlayerPropertiesPhoton.STATE, out object state))
            base.SetState((PlayerState)state);
        if (properties.TryGetValue(PlayerPropertiesPhoton.HEALTH_POINTS, out object hp))
            base.SetHealth((float)hp);
    }

    public override void ApplyDamage(IDamage damage)
    {
        if (photonView.IsMine)
        {
            base.ApplyDamage(damage);
        }
        else
        {
            if (State == PlayerState.Dead)
                return;

            // Apply damage but don't set health, this is done via OnPlayerPhotonPropertiesChangedEvent()
            LastTakenDamage = damage;
            localEventDispatcher.Invoke(new DamageTakenEvent(damage));

            if (damage is BulletDamage)
            {
                BulletDamage bulletDamage = damage as BulletDamage;
                photonView.RPC(nameof(ApplyBulletDamageRPC), RpcTarget.Others, bulletDamage.Shooter.ID, bulletDamage.HitBodyPart, bulletDamage.Amount);
            }
            else
            {
                photonView.RPC(nameof(ApplyDamageRPC), RpcTarget.Others, (int)damage.DamageType, damage.Amount);
            }
        }
    }

    [PunRPC]
    protected void ApplyDamageRPC(int damageTypeIndex, float damageAmount, PhotonMessageInfo info)
    {
        IDamage damage;
        switch ((DamageType)damageTypeIndex)
        {
            case DamageType.FallDamage:
                damage = new FallDamage(damageAmount);
                break;
            default:
                damage = new Damage(damageAmount);
                break;
        }
        base.ApplyDamage(damage);
    }

    [PunRPC]
    protected void ApplyBulletDamageRPC(string shooterId, int bodyPartIndex, float damageAmount, PhotonMessageInfo info)
    {
        IPlayer shooter = playersManager.GetPlayerById(shooterId);
        BulletDamage damage = new BulletDamage(shooter, (BodyPart)bodyPartIndex, damageAmount);
        base.ApplyDamage(damage);
    }

    public override void SetHealth(float newHealth)
    {
        base.SetHealth(newHealth);
        if (photonView.IsMine && player.IsLocal)
            (player as PhotonPlayer).NetworkClient.SetCustomProperty(PlayerPropertiesPhoton.HEALTH_POINTS, CurrentHealth);
    }

    public override void Respawn()
    {
        photonView.RPC(nameof(RespawnRPC), RpcTarget.All);
    }

    [PunRPC]
    protected void RespawnRPC(PhotonMessageInfo info)
    {
        base.Respawn();
    }

    public override Timer RespawnDelayed(float durationInSeconds)
    {
        photonView.RPC(nameof(RespawnRPC), RpcTarget.Others, durationInSeconds);
        return base.RespawnDelayed(durationInSeconds);
    }

    [PunRPC]
    protected void RespawnRPC(float duration, PhotonMessageInfo info)
    {
        base.RespawnDelayed(duration);
    }

    protected override void SetState(PlayerState state)
    {
        base.SetState(state);

        if (photonView.IsMine && player.IsLocal)
            (player as PhotonPlayer).NetworkClient.SetCustomProperty(PlayerPropertiesPhoton.STATE, (int)state);
    }
}
