using UnityEngine;

public class SetTeamTrigger : Trigger
{
    [SerializeField] private Team team = Team.None;

    protected override void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out PlayerPawnTriggerController player))
        {
            player.Player.SetTeam(team);
            base.OnTriggerEnter(other);
        }
    }
}
