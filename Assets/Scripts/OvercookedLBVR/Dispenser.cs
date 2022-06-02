using Photon.Pun;
using System;
using UnityEngine;
using Utils.Core;
using Utils.Core.Attributes;

[SelectionBase]
public class Dispenser : MonoBehaviourPun
{
    [SerializeField] private Rigidbody prefab = null;
    [SerializeField] private Transform spawnPoint = null;
    [SerializeField] private AudioSource AudioSource;

    [Header("Force")]
    [SerializeField] private bool instantiateWithForce = false;
    [SerializeField] private Vector3 forceDirection;
    [SerializeField] private float forceAmount = 1;
    [SerializeField] private ForceMode forceMode = ForceMode.Force;

    private DateTime timeLastAcitvated;
    private const float minSecondsBetweenDispense = 0.5f;

    public void DispenseObject()
    {
        photonView.RPC(nameof(RequestDispenseRPC), RpcTarget.MasterClient);
    }

    [PunRPC]
    private void RequestDispenseRPC()
    {
        if (PhotonNetwork.IsMasterClient)
        {
            TimeSpan span = DateTime.Now.Subtract(timeLastAcitvated);
            if (span.TotalSeconds > minSecondsBetweenDispense)
            {
                InstantiateObject();
                timeLastAcitvated = DateTime.Now;
            }
        }
    }

    [Button]
    private void InstantiateObject()
    { 
        GameObject instance = PhotonNetwork.Instantiate(prefab.name, spawnPoint.position, spawnPoint.rotation);
        // TODO: place this on prefabs, as this won't work for players joining late
        instance.AddComponent(typeof(DestroyOnGameStart));
        AudioSource.Play();
        if(instantiateWithForce)
        {
            if(instance.TryGetComponent(out Rigidbody rigidbody))
            {
                rigidbody.AddForce(forceDirection * forceAmount, forceMode);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if(instantiateWithForce)
            GizmosUtility.DrawArrow(spawnPoint.position, forceDirection);
    }
}


