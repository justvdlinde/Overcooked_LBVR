using Photon.Pun;

/// <summary>
/// Sends local avatar data, which is received by <see cref="NetworkedBodyDataReceiver"/> 
/// </summary>
public class NetworkedBodyDataSender : NetworkedBaseAvatar
{
    public override void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        headInterpolator.SendData(ref stream, ref info);
        leftHandInterpolator.SendData(ref stream, ref info);
        rightHandInterpolator.SendData(ref stream, ref info);
	}
}
