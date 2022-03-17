using Photon.Pun;
using System;
using UnityEngine;
using Utils.Core.Extensions;

public class Bullet : MonoBehaviour, IPoolable
{
    public bool IsBeingUsed { get; private set; }
    public Action<IPoolable> OnReturnedToPool { get; set; }

    [SerializeField] private float speed = 1;
    [SerializeField] private float damageAmount = 1;
    [Tooltip("Life time duration in seconds until object is disabled")]
    [SerializeField] private float lifeTimeDuration = 10f;
    [SerializeField] private LayerMask layerMask = new LayerMask();

    private IPlayer owner;
    private Transform parentContainer;
    private Timer lifeTimeTimer;
    private Vector3 prevPosition;
    private Vector3 stepDirection;
    private float stepSize;

    /// <summary>
    /// Only do damage if the bullet came from a local player, otherwise each remote player would do damage.
    /// </summary>
    private bool shouldDoDamage;

    public void Init(IPlayer owner, Vector3 position, Vector3 forward, float damage)
    {
        this.owner = owner;
        transform.position = position;
        transform.eulerAngles = forward;
        ResetState();
        damageAmount = damage;

        if (owner is DummyPlayer)
            shouldDoDamage = PhotonNetwork.IsMasterClient;
        else
            shouldDoDamage = owner.IsLocal;
    }

    private void Update()
    {
        transform.Translate(transform.forward * speed * Time.deltaTime, Space.World);
        CheckForCollision();
    }

    private void CheckForCollision()
    {
        stepDirection = transform.forward.normalized;
        stepSize = (transform.position - prevPosition).magnitude;

#if UNITY_EDITOR || UNITY_DEBUG
        Debug.DrawRay(prevPosition, stepDirection * stepSize, Color.red);
#endif

        if (Physics.Raycast(prevPosition, stepDirection, out RaycastHit hitInfo, stepSize, layerMask))
        {
            OnHit(hitInfo);
        }
        else
        {
            prevPosition = transform.position;
        }
    }

    private void OnHit(RaycastHit hitInfo)
    {
        if (hitInfo.collider.gameObject.TryGetInterface(out IShootable hittableObject))
        {
            if (shouldDoDamage)
            {
                BulletDamage damage;
                if (hittableObject is PlayerHitBox)
                {
                    PlayerHitBox playerHitBox = hittableObject as PlayerHitBox;
                    damage = new BulletDamage(owner, playerHitBox.BodyPartType, damageAmount * playerHitBox.DamageMultiplier);
                }
                else
                {
                    damage = new BulletDamage(owner, damageAmount);
                }
                hittableObject.Hit(damage, hitInfo.point, hitInfo.point - transform.position);
            }

            if (hittableObject.StopOnHit)
            {
                CreateHitEffect(hitInfo);
                CreateBulletHole(hitInfo);
                ReturnToPool();
            }
        }
        else
        {
            CreateHitEffect(hitInfo);
            CreateBulletHole(hitInfo);
            ReturnToPool();
        }
    }

    private void CreateHitEffect(RaycastHit hitInfo)
    {

    }

    private void CreateBulletHole(RaycastHit hitInfo)
    {

    }

    private void OnLifeTimeEnd()
    {
        if (lifeTimeTimer.IsRunning)
            lifeTimeTimer.Stop();
        ReturnToPool();
    }

    public void ResetState()
    {
        prevPosition = transform.position;
    }

    public void BecomeActive()
    {
        if(lifeTimeTimer == null)
            lifeTimeTimer = new Timer();

        parentContainer = transform.parent;
        transform.SetParent(null);
        gameObject.SetActive(true);
        lifeTimeTimer.Set(lifeTimeDuration);
        lifeTimeTimer.Start(OnLifeTimeEnd);
        IsBeingUsed = true;
        // audio.play
    }

    public void BecomeInactive()
    {
        if (lifeTimeTimer != null)
            lifeTimeTimer.Stop();

        gameObject.SetActive(false);
        IsBeingUsed = false;
        owner = null;
    }

    public void ReturnToPool()
    {
        transform.SetParent(parentContainer);
        OnReturnedToPool?.Invoke(this);
    }

    public void DestroyObject()
    {
        try
        {
            if (gameObject != null)
                Destroy(gameObject);
        }
        catch
        {
            // Left empty intentionally because bullets throw errors when play mode is exited.
        }
    }
}
