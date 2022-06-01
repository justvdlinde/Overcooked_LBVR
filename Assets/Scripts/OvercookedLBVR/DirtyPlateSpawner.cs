using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils.Core.Attributes;

public class DirtyPlateSpawner : MonoBehaviourPun
{
    [SerializeField] private Rigidbody prefab = null;
    [SerializeField] private Transform spawnPoint = null;
    [SerializeField] private AudioSource AudioSource;

    [SerializeField] private ConfigurableJoint joint = null;
    [SerializeField] private Collider checkCollider = null;
    [SerializeField] private LayerMask layermask = 0;

	private void Update()
	{
        Collider[] cols = Physics.OverlapBox(checkCollider.bounds.center, checkCollider.bounds.extents, checkCollider.transform.rotation, layermask);

        int colsCount = -1;
        float colHeight = 0.0225f;

        for (int i = 0; i < cols.Length; i++)
		{
            if (cols[i].isTrigger && cols[i].tag != "plate")
                continue;

            //colHeight = cols[i].bounds.size.y;
            colsCount++;
		}

        Vector3 targPos = joint.targetPosition;
        targPos.y = Mathf.Max(0, colsCount) * colHeight;
        joint.targetPosition = targPos;
	}

	public void DispenseObject(Rigidbody obj)
    {
        GameObject g = PhotonNetwork.Instantiate(prefab.name, spawnPoint.position, spawnPoint.rotation);
        g.AddComponent(typeof(DestroyOnGameStart));
        photonView.RPC(nameof(DispenseObjectRPC), RpcTarget.All);
        photonView.TransferOwnership(-1);

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
