using UnityEngine;

public class DamageTrigger : Trigger
{
    [SerializeField] private float amount = 10;

    protected override void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerPawnTriggerController player))
        {
            player.Pawn.HealthController.ApplyDamage(new Damage(amount));
            base.OnTriggerEnter(other);
        }
    }
}
