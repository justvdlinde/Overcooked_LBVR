using Photon.Pun;
using UnityEngine;
using Utils.Core.Attributes;

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
        photonView.RPC(nameof(DispenseObjectRPC), RpcTarget.All);

        //if(g.TryGetComponent(out PhotonView photonView))
        //{
        //    photonView.TransferOwnership(-1);
        //}
        //instance.AddForce(instance.transform.forward * forceAmount, forceMode);
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


