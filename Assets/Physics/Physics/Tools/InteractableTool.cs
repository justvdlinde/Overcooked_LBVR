using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PhysicsCharacter;
using System;
using UnityEngine.Events;
using Photon.Pun;

public class InteractableTool : MonoBehaviourPun
{
    [SerializeField] private Tool connectedTool = null;

	public bool previousState = false;

	private bool isTriggerHeld = false;
	private bool wasTriggerHeld = false;

	[SerializeField] public UnityEvent UpEvent = null;
	[SerializeField] public UnityEvent HeldEvent = null;
	[SerializeField] public UnityEvent DownEvent = null;

	private void Update()
	{
		if (photonView.IsMine)
		{
			bool isBeingHeld = connectedTool.IsBeingHeld();

			float val = XRInput.GetTriggerButtonValue(connectedTool.GetHeldHand());

			if (val < 0.1f)
				isTriggerHeld = false;
			else
				isTriggerHeld = true;

			if (isBeingHeld && wasTriggerHeld == false && isTriggerHeld)
			{
				OnDownEvent();
			}
			else if (previousState == true && wasTriggerHeld == true && !isTriggerHeld)
			{
				OnUpEvent();
			}
			if (connectedTool.IsBeingHeld() && isTriggerHeld)
			{
				HeldEvent?.Invoke();
			}

			if (previousState == false && !connectedTool.IsBeingHeld())
			{
				OnUpEvent();
			}

			previousState = connectedTool.IsBeingHeld();
			wasTriggerHeld = isTriggerHeld;
		}
	}

	public void OnUpEvent()
    {
		photonView.RPC(nameof(OnUpEventRPC), RpcTarget.All);
    }

	[PunRPC]
	private void OnUpEventRPC(PhotonMessageInfo info)
    {
		UpEvent?.Invoke();
	}

	private void OnDownEvent()
    {
		photonView.RPC(nameof(OnDownEventRPC), RpcTarget.All);
	}

	[PunRPC]
	private void OnDownEventRPC(PhotonMessageInfo info)
    {
		DownEvent?.Invoke();
    }
}
