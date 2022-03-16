using Photon.Pun;
using UnityEngine;
using Utils.Core.Attributes;

public class Dispenser : MonoBehaviour
{
    [SerializeField] private Rigidbody prefab = null;
    [SerializeField] private Transform spawnPoint = null;
    [SerializeField] private float forceAmount = 1;
    [SerializeField] private ForceMode forceMode = ForceMode.Force;

    public void DispenseObject(Rigidbody obj)
    {
        //Rigidbody instance = Instantiate(obj, spawnPoint.position, spawnPoint.rotation);
        GameObject g = PhotonNetwork.Instantiate(prefab.name, spawnPoint.position, spawnPoint.rotation);
        //if(g.TryGetComponent(out Rigidbody rigidbody))
        //    rigidbody.AddForce(rigidbody.transform.forward * forceAmount, forceMode);
    }

    [Button]
    public void DispenseObject()
    {
        DispenseObject(prefab);
    }
}
