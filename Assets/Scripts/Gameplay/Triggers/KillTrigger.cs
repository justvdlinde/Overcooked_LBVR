using UnityEngine;

public class KillTrigger : Trigger
{
    protected override void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerPawnTriggerController player))
        {
            HealthController healthController = player.Pawn.HealthController;
            if (healthController.State == PlayerState.Alive)
            {
                healthController.ApplyDamage(new Damage(healthController.MaxHealth));
                base.OnTriggerEnter(other);
            }
        }
    }
}
