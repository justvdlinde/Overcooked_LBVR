using Photon.Pun;
using UnityEngine;
using Utils.Core.Attributes;

public class Dispenser : MonoBehaviour
{
    [SerializeField] private Rigidbody prefab = null;
    [SerializeField] private Transform spawnPoint = null;
    [SerializeField] private float forceAmount = 1;
    [SerializeField] private ForceMode forceMode = ForceMode.Force;
    [SerializeField] private AudioSource AudioSource;


    public void DispenseObject(Rigidbody obj)
    {
        GameObject g = PhotonNetwork.Instantiate(prefab.name, spawnPoint.position, spawnPoint.rotation);
        AudioSource.Play();
        //instance.AddForce(instance.transform.forward * forceAmount, forceMode);
    }

    [Button]
    public void DispenseObject()
    { 
        DispenseObject(prefab);

    }
}


