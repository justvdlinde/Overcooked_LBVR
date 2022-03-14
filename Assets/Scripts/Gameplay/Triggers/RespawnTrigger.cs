using UnityEngine;

public class RespawnTrigger : Trigger
{
    [SerializeField] private bool respawnDelayed = false;
    [Tooltip("Duration used when respawnDelayed is set to true")]
    [SerializeField] private float respawnDuration = 2;

    protected override void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerPawnTriggerController player))
        {
            PlayerHealthController healthController = player.Pawn.HealthController;
            if (healthController.State == PlayerState.Dead && healthController.CanRespawn)
            {
                if(respawnDelayed)
                    healthController.RespawnDelayed(respawnDuration);
                else
                    healthController.Respawn();
            }
            base.OnTriggerEnter(other);
        }
    }
}
