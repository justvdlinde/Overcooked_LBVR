using Photon.Pun;
using UnityEngine;

/// <summary>
/// Receives and interpolates remote avatar body data that's sent by <see cref="NetworkedBodyDataSender"/> 
/// </summary>
public class NetworkedBodyDataReceiver : NetworkedBaseAvatar
{
    [SerializeField] private Transform bodyRoot = null;

    protected void Update()
    {
        headInterpolator.Update();
        leftHandInterpolator.Update();
		rightHandInterpolator.Update();

        Vector3 rootPos = body.Head.position;
        rootPos.y = 0;
        bodyRoot.transform.position = rootPos;
	}

    public override void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        headInterpolator.OnDataReceived(ref stream, ref info);
        leftHandInterpolator.OnDataReceived(ref stream, ref info);
        rightHandInterpolator.OnDataReceived(ref stream, ref info);
	}
}
