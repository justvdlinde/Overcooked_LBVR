using System;
using UnityEngine;

public class PlayerClippingPrevention : MonoBehaviour
{
    [SerializeField] private PlayerCollisionDetector headCollision = null;
    [SerializeField] private PlayerCollisionDetector leftHandCollision = null;
    [SerializeField] private PlayerCollisionDetector rightHandCollision = null;

    public bool HeadIsColliding => headCollision.IsColliding;
    public bool RightHandIsColliding => rightHandCollision.IsColliding;
    public bool LeftHandIsColliding => leftHandCollision.IsColliding;

    private void OnEnable()
    {
        headCollision.EnterEvent += OnCollisionEnterDetected;
        headCollision.ExitEvent += OnCollisionExitDetected;

        leftHandCollision.EnterEvent += OnCollisionEnterDetected;
        leftHandCollision.ExitEvent += OnCollisionExitDetected;

        rightHandCollision.EnterEvent += OnCollisionEnterDetected;
        rightHandCollision.ExitEvent += OnCollisionExitDetected;
    }

    private void OnDisable()
    {
        headCollision.EnterEvent += OnCollisionEnterDetected;
        headCollision.ExitEvent += OnCollisionExitDetected;

        leftHandCollision.EnterEvent += OnCollisionEnterDetected;
        leftHandCollision.ExitEvent += OnCollisionExitDetected;

        rightHandCollision.EnterEvent += OnCollisionEnterDetected;
        rightHandCollision.ExitEvent += OnCollisionExitDetected;
    }

#if UNITY_EDITOR || UNITY_DEBUG
    private void Update()
    {
        GUIWorldSpace.Log("HeadIsColliding: " + HeadIsColliding);    
        GUIWorldSpace.Log("LeftHandIsColliding: " + LeftHandIsColliding);    
        GUIWorldSpace.Log("RightHandIsColliding: " + RightHandIsColliding);    
    }
#endif

    private void OnCollisionEnterDetected(BodyPart bodyPart, Collider collider)
    {

    }

    private void OnCollisionExitDetected(BodyPart bodyPart, Collider collider)
    {

    }
}

