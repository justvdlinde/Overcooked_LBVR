using Photon.Pun;
using UnityEngine;
using Utils.Core.Attributes;

/// <summary>
/// Receives input values from remote player after which it lerps and applies it to <see cref="input"/>
/// </summary>
public class NetworkedInputReceiver : NetworkedInputBase, IPunObservable
{
    [SerializeField] private float lerpSpeed = 50;

    [SerializeField, ReadOnly] private float networkedTriggerButtonRight;
    [SerializeField, ReadOnly] private float networkedTriggerButtonLeft;

    private RemotePlayerInput input;

    public void InjectDependencies(RemotePlayerInput input)
    {
        this.input = input;
    }

    private void Update()
    {
        input.TriggerButtonRight = Mathf.Lerp(input.TriggerButtonRight, networkedTriggerButtonRight, Time.deltaTime * lerpSpeed);
        input.TriggerButtonLeft = Mathf.Lerp(input.TriggerButtonLeft, networkedTriggerButtonLeft, Time.deltaTime * lerpSpeed);
    }

    public override void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        //float lag = Mathf.Abs((float)(PhotonNetwork.Time - info.SentServerTime));

        // 0:
        networkedTriggerButtonRight = (float)stream.ReceiveNext();
        // 1:
        networkedTriggerButtonLeft = (float)stream.ReceiveNext();
    }
}
