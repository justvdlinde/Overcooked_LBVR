using UnityEngine;

public class HealthController : MonoBehaviour
{
    public PlayerState State { get; protected set; }
    public float CurrentHealth { get; protected set; }
    public bool CanRespawn { get; protected set; }
    public IDamage LastTakenDamage { get; protected set; }
    public float MaxHealth => maxHealth;

    public delegate void HealthChangedEventDelegate(float prevHealth, float newHealth);
    public event HealthChangedEventDelegate HealthChangedEvent;

    [SerializeField] protected float maxHealth = 100;
    [SerializeField] protected float startingHealth = 100;
    [SerializeField] protected float respawnDelay = 1f;

    protected virtual void Start()
    {
        startingHealth = Mathf.Clamp(startingHealth, 0, MaxHealth);
        SetHealth(startingHealth);
        SetState(PlayerState.Alive);
    }

    public virtual void ApplyDamage(IDamage damage)
    {
        if (State == PlayerState.Dead)
            return;

        if (damage.Amount < 0)
            damage.Amount = 0;

        LastTakenDamage = damage;
        AdjustHealth(-damage.Amount);
    }

    public virtual void AdjustHealth(float amount)
    {
        SetHealth(CurrentHealth + amount);
    }

    public virtual void SetHealth(float newHealth)
    {
        if (newHealth == CurrentHealth)
            return;

        float prevHealth = CurrentHealth;
        CurrentHealth = Mathf.Clamp(newHealth, 0, MaxHealth);
        RaiseHealthChangedEvent(prevHealth, CurrentHealth);
        if (CurrentHealth <= 0)
            OnHpReachedZero();
    }

    protected virtual void OnHpReachedZero()
    {
        if (State == PlayerState.Dead)
            return;

        SetState(PlayerState.Dead);
        CanRespawn = false;
        Timer respawnDelayTimer = new Timer();
        respawnDelayTimer.Set(respawnDelay);
        respawnDelayTimer.Start(() => CanRespawn = true);
    }

    public virtual void Respawn()
    {
        if (!CanRespawn || State != PlayerState.Dead)
            return;

        SetHealth(MaxHealth);
        SetState(PlayerState.Alive);
        LastTakenDamage = null;
    }

    protected virtual void SetState(PlayerState state)
    {
        State = state;
    }

    protected void RaiseHealthChangedEvent(float prevHealth, float newHealth)
    {
        HealthChangedEvent?.Invoke(prevHealth, CurrentHealth);
    }
}
