using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PhysicsCharacter;
using System;
using UnityEngine.Events;

public class InteractableTool : MonoBehaviour
{
    [SerializeField] private Tool connectedTool = null;

	public bool isActive = false;
	public bool previousState = false;

	private bool isTriggerHeld = false;
	private bool wasTriggerHeld = false;

	[SerializeField] public UnityEvent UpEvent = null;
	[SerializeField] public UnityEvent HeldEvent = null;
	[SerializeField] public UnityEvent DownEvent = null;

	private void Update()
	{
		bool isBeingHeld = connectedTool.IsBeingHeld();

		float val = XRInput.GetTriggerButtonValue(connectedTool.GetHeldHand());

		if (val < 0.1f)
			isTriggerHeld = false;
		else
			isTriggerHeld = true;

		isActive = isBeingHeld;
		if (isBeingHeld && wasTriggerHeld == false && isTriggerHeld)
		{
			DownEvent?.Invoke();
		}
		else if (previousState == true && wasTriggerHeld == true && !isTriggerHeld)
		{
			UpEvent?.Invoke();
		}
		if (connectedTool.IsBeingHeld() && isTriggerHeld)
		{
			HeldEvent?.Invoke();
		}

		if(previousState == false && !connectedTool.IsBeingHeld())
		{
			UpEvent?.Invoke();
		}

		previousState = connectedTool.IsBeingHeld();
		wasTriggerHeld = isTriggerHeld;
	}
}
