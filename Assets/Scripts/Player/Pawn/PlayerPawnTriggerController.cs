using UnityEngine;

public class PlayerPawnTriggerController : MonoBehaviour
{
    public PlayerPawn Pawn => pawn;
    public IPlayer Player => pawn.Owner;

    [SerializeField] private PlayerPawn pawn = null;
}
