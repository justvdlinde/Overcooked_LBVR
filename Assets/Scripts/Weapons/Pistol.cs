using Photon.Pun;
using UnityEngine;
using Utils.Core;
using Utils.Core.Attributes;

[SelectionBase]
public class Pistol : MonoBehaviourPun
{
	private const int BULLET_OBJECT_POOL_SIZE = 10;
	private const float TRIGGER_VAL = 0.8f;
	private const float IRON_SIGHT_MAX_RAY_DISTANCE = 0.1f;

    public IPlayer Owner { get; protected set; }
	public bool CanShoot => isInCooldown == false;
	public Timer CooldownTimer { get; protected set; }

	[SerializeField] private Bullet bulletPrefab = null;
	[SerializeField] private Transform bulletExit = null;
	[Tooltip("Cooldown duration between shots, measured in seconds")]
	[SerializeField] private float cooldownDuration = 0.5f;
#pragma warning disable CS0414
	[SerializeField] private float hapticsDuration = 0.3f;
#pragma warning restore CS0414
	[SerializeField] private Transform ironSightTip = null;
	[SerializeField] private ParticleSystem muzzleFlash = null;
	[SerializeField] private LayerMask hittableMask = 0;
	[SerializeField] private float damage = 40;

	private BulletObjectPool objectPool;
	private bool isInCooldown;

	public void InjectDependencies(IPlayer player)
    {
		Owner = player;

		// TODO: test if this works with dummy?
		if (player is PhotonPlayer)
		{
			photonView.TransferOwnership((player as PhotonPlayer).NetworkClient);
			photonView.Setup();
		}
	}

	private void Start()
	{
		objectPool = new BulletObjectPool(BULLET_OBJECT_POOL_SIZE, bulletPrefab, transform);
		objectPool.FillPool();
		CooldownTimer = new Timer();
	}

	private void OnEnable()
	{
		isInCooldown = false;
	}

	[Button]
	public void Shoot()
	{
		if (!CanShoot)
			return;

		RaycastHit hit;
		Ray ray = new Ray(ironSightTip.position, ironSightTip.forward);
		if (Physics.Raycast(ray, out hit, 100f, hittableMask))
		{
			if (Vector3.Distance(ironSightTip.position, hit.point) >= IRON_SIGHT_MAX_RAY_DISTANCE)
				bulletExit.transform.LookAt(hit.point);
			else
				bulletExit.transform.forward = ironSightTip.transform.forward;
		}
		photonView.RPC(nameof(ShootRPC), RpcTarget.All, bulletExit.transform.position, bulletExit.transform.eulerAngles);
	}

	[PunRPC]
	private void ShootRPC(Vector3 position, Vector3 forward, PhotonMessageInfo info)
	{
		if (objectPool == null)
			throw new System.Exception("Object pool is null");

		Bullet bullet = objectPool.GetNextPoolableObject();

		if (bullet == null)
			throw new System.Exception("Bullet is null");

		bullet.Init(Owner, position, forward, damage);

		if (muzzleFlash != null)
		{
			if (muzzleFlash.isPlaying)
				muzzleFlash.Stop();
			muzzleFlash.Play(true);
		}

		if (photonView.IsMine && Owner.IsLocal)
		{
			StartCooldown(cooldownDuration);
//#if UNITY_ANDROID
//			hapticsCoroutine = StartCoroutine(ControllerHaptics.PlayHapticsLerpedCoroutine(hapticsDuration, hapticsCurve, Hand));
//#endif
		}
	}

	private void StartCooldown(float duration)
	{
		isInCooldown = true;
		CooldownTimer.Set(duration);
		CooldownTimer.Start(() => isInCooldown = false);
	}

	public void SetTriggerValue(float trigger)
    {
		// move the pistol's trigger
    }

	private void OnDrawGizmos()
	{
		if(bulletExit != null)
			GizmosUtility.DrawArrow(bulletExit.position, bulletExit.forward * 0.25f, Color.red, 0.1f);
	}
}
