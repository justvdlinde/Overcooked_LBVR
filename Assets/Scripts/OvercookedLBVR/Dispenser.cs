using Photon.Pun;
using System;
using System.Collections;
using UnityEngine;
using Utils.Core;
using Utils.Core.Attributes;

[SelectionBase]
public class Dispenser : MonoBehaviourPun
{
    [SerializeField] protected Rigidbody prefab = null;
    [SerializeField] protected Transform spawnPoint = null;
    [SerializeField] private AudioSource AudioSource;

    [Header("Force")]
    [SerializeField] private bool instantiateWithForce = false;
    [SerializeField] private float forceAmount = 1;
    [SerializeField] private ForceMode forceMode = ForceMode.Force;

    [SerializeField] private Animator anim = null;

    private DateTime timeLastAcitvated;
    private const float minSecondsBetweenDispense = 3.0f;

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
                StartDelay();
                timeLastAcitvated = DateTime.Now;
            }
        }
    }

    [Button]
    private void StartDelay()
    {
        StartCoroutine(InstantiateDelayed());
    }

    private IEnumerator InstantiateDelayed()
	{
        photonView.RPC(nameof(DoAnimationRPC), RpcTarget.All);
        yield return new WaitForSeconds(0.2f);
        InstantiateObject();
    }

    protected virtual GameObject InstantiateObject()
    {
        GameObject instance = PhotonNetwork.Instantiate(prefab.name, spawnPoint.position, Quaternion.identity);
        AudioSource.Play();
        if (instantiateWithForce)
        {
            if (instance.TryGetComponent(out Rigidbody rigidbody))
            {
                float forceAmt = forceAmount + Mathf.Lerp(forceAmount * -0.1f, forceAmount * 0.1f, UnityEngine.Random.value);
                rigidbody.AddForce(spawnPoint.forward * forceAmt, forceMode);
                rigidbody.AddTorque(UnityEngine.Random.insideUnitSphere * forceAmt * 0.01f);
            }
        }
        return instance;
    }

    [PunRPC]
    private void DoAnimationRPC()
	{
        if (anim != null)
            anim.SetTrigger("Dispense");
    }

    private void OnDrawGizmosSelected()
    {
        if(instantiateWithForce)
            GizmosUtility.DrawArrow(spawnPoint.position, spawnPoint.forward.normalized * 0.15f, 0.05f);
    }
}


