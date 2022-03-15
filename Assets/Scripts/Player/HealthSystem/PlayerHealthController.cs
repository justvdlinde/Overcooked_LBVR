using System.Collections;
using UnityEngine;
using Utils.Core.Events;

public class PlayerHealthController : HealthController 
{
    public bool IsRespawning { get; protected set; }
    public PlayerDeathInfo LastDeathInfo { get; protected set; }
    public Timer RespawnTimer { get; protected set; }

    public delegate void RespawnEventDelegate(float newHealth);
    public event RespawnEventDelegate RespawnEvent;

    public delegate void DeathEventDelegate(PlayerDeathInfo info);
    public event DeathEventDelegate DeathEvent;

    protected IPlayer player;
    protected EventDispatcher localEventDispatcher;
    protected GlobalEventDispatcher globalEventDispatcher;

    public void InjectDependencies(IPlayer player, EventDispatcher localEventDispatcher, GlobalEventDispatcher globalEventDispatcher)
    {
        this.player = player;
        this.localEventDispatcher = localEventDispatcher;
        this.globalEventDispatcher = globalEventDispatcher;
    }

    public override void ApplyDamage(IDamage damage)
    {
        if (State == PlayerState.Dead)
            return;

        base.ApplyDamage(damage);
        localEventDispatcher.Invoke(new DamageTakenEvent(damage));
    }

    protected override void OnHpReachedZero()
    {
        if (State == PlayerState.Dead)
            return;

        base.OnHpReachedZero();

        if (LastTakenDamage is BulletDamage)
            LastDeathInfo = new PlayerKilledInfo(player, LastTakenDamage, (LastTakenDamage as BulletDamage).Shooter);
        else
            LastDeathInfo = new PlayerDeathInfo(player, LastTakenDamage);

        PlayerDeathEvent deathEvent = new PlayerDeathEvent(LastDeathInfo);
        DeathEvent?.Invoke(LastDeathInfo);
        localEventDispatcher.Invoke(deathEvent);
        globalEventDispatcher.Invoke(deathEvent);
    }

    public override void Respawn()
    {
        if (!CanRespawn)
            return;

        base.Respawn();
        PlayerRespawnEvent respawnEvent = new PlayerRespawnEvent(player, CurrentHealth);
        RespawnEvent?.Invoke(CurrentHealth);
        localEventDispatcher.Invoke(respawnEvent);
        globalEventDispatcher.Invoke(respawnEvent);
        IsRespawning = false;
    }

    public virtual Timer RespawnDelayed(float durationInSeconds)
    {
        RespawnTimer = new Timer();
        RespawnTimer.Set(durationInSeconds);
        RespawnTimer.Start(Respawn);
        StartCoroutine(LerpHealthCoroutine(RespawnTimer));
        IsRespawning = true;
        return RespawnTimer;
    }

    protected IEnumerator LerpHealthCoroutine(Timer timer)
    {
        float startingHealth = CurrentHealth;
        while(timer.IsRunning)
        {
            float lerp = timer.ElapsedTime / timer.Duration;
            float prevHealth = CurrentHealth;
            float newHealth = Mathf.Lerp(startingHealth, MaxHealth, lerp);
            CurrentHealth = newHealth;
            RaiseHealthChangedEvent(prevHealth, newHealth);
            yield return null;
        }
    }
}
