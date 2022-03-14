using UnityEngine;

public class RegenTrigger : Trigger
{
    [SerializeField] private float amount = 10;

    protected override void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerPawnTriggerController player))
        {
            player.Pawn.HealthController.AdjustHealth(amount);
            base.OnTriggerEnter(other);
        }
    }
}
