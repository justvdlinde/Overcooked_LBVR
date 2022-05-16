﻿using Photon.Pun;
using UnityEngine;

/// <summary>
/// Instantiates a <see cref="PhotonPlayerProxy"/> gameobject when joining a Photon room, this object is synced on remote players and will 
/// instantiate a remote version of this <see cref="PlayerPawn"/> prefab. This is needed because Photon doesn't allow for instantiating 
/// different prfabs for local and remote players
/// </summary>
public class LocalPlayerPawnPhotonManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private PlayerPawn pawn = null;
    [SerializeField] private PhotonView[] childViews = null;

    private PhotonPlayerProxy proxyInstance;

    public override void OnJoinedRoom()
    {
        if (PhotonNetwork.OfflineMode)
            return;

        photonView.Setup();
        for (int i = 0; i < childViews.Length; i++)
        {
            PhotonView view = childViews[i];
            Debug.Log("Owner.ActorNumber " + photonView.Owner.ActorNumber);
            view.ViewID = photonView.ViewID + (i + 1);
            view.TransferOwnership(photonView.Owner);
        }

        object[] instantiationData = new object[]
        {
            pawn.transform.position,
            pawn.transform.rotation,
            pawn.Owner.ID,
            pawn.PhotonView.ViewID
        };

        proxyInstance = PhotonNetwork.Instantiate("PlayerProxy", Vector3.zero, Quaternion.identity, 0, instantiationData).GetComponent<PhotonPlayerProxy>();
        proxyInstance.transform.SetParent(pawn.transform);
    }

    public override void OnLeftRoom()
    {
        if (proxyInstance != null)
            Destroy(proxyInstance.gameObject);
    }
}
