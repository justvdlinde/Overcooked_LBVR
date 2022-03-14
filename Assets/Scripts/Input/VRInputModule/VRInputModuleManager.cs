using UnityEngine;
using Utils.Core.Attributes;

public class VRInputModuleManager : MonoBehaviour
{
    [SerializeField] private VRInputModule inputModule = null;
    [SerializeField] private Hand startingHand = Hand.Right;
    [SerializeField] private Transform rightLaserOrigin = null;
    [SerializeField] private Transform leftLaserOrigin = null;

    private void Awake()
    {
        SetupInputModule(startingHand);
    }

    public void SetupInputModule(Hand hand)
    {
        Transform point = hand == Hand.Right ? rightLaserOrigin : leftLaserOrigin;
        inputModule.rayTransform = point;
        inputModule.hand = hand;
    }

    [Button]
    public void SwitchHand()
    {
        SetupInputModule(startingHand == Hand.Left ? Hand.Right : Hand.Left);
    }
}
