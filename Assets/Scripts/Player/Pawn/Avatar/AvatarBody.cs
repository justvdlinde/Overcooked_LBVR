using System;
using UnityEngine;

/// <summary>
/// MonoBehaviour container for head and hands
/// </summary>
[System.Serializable]
public class AvatarBody : MonoBehaviour
{
    public Transform Head => head;
    [SerializeField] private Transform head = null;
    public Transform LeftHand => leftHand;
    [SerializeField] private Transform leftHand = null;
    public Transform RightHand => rightHand;
    [SerializeField] private Transform rightHand = null;
}

/// <summary>
/// Container class for head and hands transform data
/// </summary>
[System.Serializable]
public class AvatarDataContainer
{
    public TransformPosRotData headData;
    public TransformPosRotData leftHandData;
    public TransformPosRotData rightHandData;
}

/// <summary>
/// Container class for position and rotation
/// </summary>
[System.Serializable]
public struct TransformPosRotData
{
    public Vector3 position;
    public Quaternion rotation;

    public TransformPosRotData(Vector3 position, Quaternion rotation)
    {
        this.position = position;
        this.rotation = rotation;
    }

    public void Update(Vector3 position, Quaternion rotation)
    {
        this.position = position;
        this.rotation = rotation;
    }
}