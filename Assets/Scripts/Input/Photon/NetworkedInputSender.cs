using Photon.Pun;

/// <summary>
/// Sends necessary values from <see cref="input"/> over network
/// </summary>
public class NetworkedInputSender : NetworkedInputBase, IPunObservable
{
    private PlayerControls input;

    public void InjectDependencies(PlayerControls input)
    {
        this.input = input;
    }

    // In case of one-handed mode adjust values being sent:
    public override void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
    {
        // 0:
        stream.SendNext(input.RightHand.Trigger.ReadValue<float>());
        // 1:
        stream.SendNext(input.LeftHand.Trigger.ReadValue<float>());
    }
}
