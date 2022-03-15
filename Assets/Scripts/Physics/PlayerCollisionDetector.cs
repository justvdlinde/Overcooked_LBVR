using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Detects collision on objects
/// </summary>
public class PlayerCollisionDetector : Trigger
{
    public bool IsColliding { get; protected set; }
    public List<Collider> Collisions { get; protected set; } = new List<Collider>();
    [SerializeField] private BodyPart bodyPart = BodyPart.Undefined;

    public delegate void PlayerCollisionDetectedDelegate(BodyPart bodyPart, Collider collider);

    public new event PlayerCollisionDetectedDelegate EnterEvent;
    public new event PlayerCollisionDetectedDelegate ExitEvent;

    protected override void OnTriggerEnter(Collider collider)
    {
        if (!Collisions.Contains(collider))
            Collisions.Add(collider);

        if (!IsColliding)
        {
            IsColliding = true;
        }

        base.OnTriggerEnter(collider);
        EnterEvent?.Invoke(bodyPart, collider);
    }

    protected override void OnTriggerExit(Collider collider)
    {
        if (Collisions.Contains(collider))
            Collisions.Remove(collider);

        if (Collisions.Count == 0)
        {
            IsColliding = false;
        }

        base.OnTriggerExit(collider);
        ExitEvent?.Invoke(bodyPart, collider);
    }
}
