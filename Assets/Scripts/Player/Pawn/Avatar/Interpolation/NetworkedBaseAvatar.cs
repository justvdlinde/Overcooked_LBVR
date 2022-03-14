using Photon.Pun;
using UnityEngine;

/// <summary>
/// Base class for networked avatar interpolator.
/// The remote and local versions need to inherit from this base class, otherwise Photon won't be able to serialize it's values.
/// </summary>
public abstract class NetworkedBaseAvatar : MonoBehaviour, IPunObservable
{
    [SerializeField] protected AvatarBody body;

    protected TransformInterpolator headInterpolator;
    protected TransformInterpolator leftHandInterpolator;
    protected TransformInterpolator rightHandInterpolator;

    protected virtual void Awake()
    {
        headInterpolator = new TransformInterpolator(body.Head);
        leftHandInterpolator = new TransformInterpolator(body.LeftHand);
        rightHandInterpolator = new TransformInterpolator(body.RightHand);
    }

    public abstract void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info);
}
