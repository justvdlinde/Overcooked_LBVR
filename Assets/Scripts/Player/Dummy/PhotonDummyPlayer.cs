using Photon.Pun;
using UnityEngine;
using Utils.Core.Injection;

public class PhotonDummyPlayer : DummyPlayer
{
    public PhotonDummyPlayer(DependencyInjector injector, string id, bool instantiatePawn) : base(injector, id, instantiatePawn) { }

    protected override PlayerPawn InstantiatePlayerPawn()
    {
        object[] data = new object[]
        {
            ID
        };
        GameObject pawnObject = PhotonNetwork.Instantiate("DummyPlayer", Vector3.zero, Quaternion.identity, 0, data);
        PlayerPawn pawn = pawnObject.GetComponent<PlayerPawn>();
        Injector.InjectGameObject(pawnObject);
        pawn.Setup(this);
        return pawn;
    }

    public override void Dispose()
    {
        if (Pawn != null)
        {
            PhotonNetwork.Destroy(Pawn.gameObject);
            Pawn = null;
        }
        base.Dispose();
    }
}
