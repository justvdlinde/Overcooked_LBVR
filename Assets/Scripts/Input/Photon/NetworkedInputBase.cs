using Photon.Pun;
using UnityEngine;

/// <summary>
/// Abstract class for networked input.
/// The remote and local versions need to inherit from this base class, otherwise Photon won't be able to serialize it's values.
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class NetworkedInputBase : MonoBehaviour, IPunObservable
{

    public abstract void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info);
}
