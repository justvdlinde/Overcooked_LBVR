using Photon.Pun;
using UnityEngine;

public class RemotePlayerpawnPhotonManager : MonoBehaviourPunCallbacks
{
    [SerializeField] private PhotonView[] childViews = null;

    private void Start()
    {
        SetupPhotonViews();
    }

    private void SetupPhotonViews()
    {
        if (PhotonNetwork.OfflineMode)
            return;

        for (int i = 0; i < childViews.Length; i++)
        {
            PhotonView view = childViews[i];
            view.ViewID = photonView.ViewID + (i + 1);
            view.TransferOwnership(photonView.Owner);
        }
    }
}
