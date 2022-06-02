using Photon.Pun;
using System;
using System.Collections;
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
    [SerializeField] private float forceAmount = 1;
    [SerializeField] private ForceMode forceMode = ForceMode.Force;

    [SerializeField] private Animator anim = null;

    private DateTime timeLastAcitvated;
    private const float minSecondsBetweenDispense = 4.5f;

    private bool isDispensing = false;

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
        StartCoroutine(InstantiateDelayed());
    }

    private IEnumerator InstantiateDelayed()
	{
        if (anim != null)
            anim.SetTrigger("Dispense");

        yield return new WaitForSeconds(0.25f);

        GameObject instance = PhotonNetwork.Instantiate(prefab.name, spawnPoint.position, spawnPoint.rotation);
        // TODO: place this on prefabs, as this won't work for players joining late
        instance.AddComponent(typeof(DestroyOnGameStart));
        AudioSource.Play();
        if (instantiateWithForce)
        {
            if (instance.TryGetComponent(out Rigidbody rigidbody))
            {
                rigidbody.AddForce(spawnPoint.forward * forceAmount, forceMode);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        if(instantiateWithForce)
            GizmosUtility.DrawArrow(spawnPoint.position, spawnPoint.forward.normalized * 0.15f, 0.05f);
    }
}


