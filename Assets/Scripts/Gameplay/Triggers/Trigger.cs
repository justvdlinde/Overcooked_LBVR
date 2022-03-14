using System;
using UnityEngine;

/// <summary>
/// Simple class for detecting triggers, which can be usefull when the trigger box is on a child object.
/// Use the events to receive collision events.
/// </summary>
[SelectionBase]
[RequireComponent(typeof(Collider))]
public class Trigger : MonoBehaviour
{
    public Action<Collider> EnterEvent;
    public Action<Collider> StayEvent;
    public Action<Collider> ExitEvent;

    public Action disableEvent;

    protected virtual void OnDisable()
    {
        // This is needed because OnTriggerExit isn't called when the trigger is disabled
        disableEvent?.Invoke();
    }

    protected virtual void OnTriggerEnter(Collider other)
    {
        EnterEvent?.Invoke(other);
    }

    protected virtual void OnTriggerStay(Collider other)
    {
        StayEvent?.Invoke(other);
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        ExitEvent?.Invoke(other);
    }
}
