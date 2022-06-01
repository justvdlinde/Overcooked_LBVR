using Photon.Pun;
using UnityEngine;
using Utils.Core.Attributes;

[SelectionBase]
public class Dispenser : MonoBehaviourPun
{
    [SerializeField] private Rigidbody prefab = null;
    [SerializeField] private Transform spawnPoint = null;
    [SerializeField] private float forceAmount = 1;
    [SerializeField] private ForceMode forceMode = ForceMode.Force;
    [SerializeField] private AudioSource AudioSource;

	public void DispenseObject(Rigidbody obj)
    {
        GameObject g = PhotonNetwork.Instantiate(prefab.name, spawnPoint.position, spawnPoint.rotation);
        g.AddComponent(typeof(DestroyOnGameStart));
        photonView.RPC(nameof(DispenseObjectRPC), RpcTarget.All);
        photonView.TransferOwnership(-1);
    }

    [PunRPC]
    private void DispenseObjectRPC(PhotonMessageInfo info)
    {
        AudioSource.Play();
    }

    [Button]
    public void DispenseObject()
    { 
        DispenseObject(prefab);

    }
}


