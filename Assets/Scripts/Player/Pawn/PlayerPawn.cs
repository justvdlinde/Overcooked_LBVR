using Photon.Pun;
using UnityEngine;

/// <summary>
/// 'Root' object of player pawn, use this to get component references on the pawn gameobject
/// </summary>
public class PlayerPawn : MonoBehaviour
{
    public IPlayer Owner { get; private set; }

    public PhotonView PhotonView => photonView;
    [SerializeField] private PhotonView photonView;

    public AvatarBody Body => body;
    [SerializeField] private AvatarBody body = null;

    public PlayerHealthController HealthController => healthController;
    [SerializeField] private PlayerHealthController healthController = null;

    public void Setup(IPlayer player)
    {
        Owner = player;

        if(player.IsLocal)
            photonView.Setup();
    }

    private void Start()
    {
        DontDestroyOnLoad(gameObject);
    }
}
