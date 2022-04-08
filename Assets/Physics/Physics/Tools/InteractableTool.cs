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
	public bool isTriggerHeld = false;
	public bool isTriggerHeldPreviousFrame = false;

	[SerializeField] public UnityEvent UpEvent = null;
	[SerializeField] public UnityEvent HeldEvent = null;
	[SerializeField] public UnityEvent DownEvent = null;

	private void Update()
	{
		if (!photonView.IsMine)
			return;

		// TODO: use input system
		float triggerPress = XRInput.GetTriggerButtonValue(connectedTool.GetHeldHand());

		if (triggerPress < 0.1f)
			isTriggerHeld = false;
		else
			isTriggerHeld = true;

		if (connectedTool.IsBeingHeld() && isTriggerHeldPreviousFrame == false && isTriggerHeld)
		{
			OnDownEvent();
		}
		else if (previousState == true && isTriggerHeldPreviousFrame == true && !isTriggerHeld)
		{
			OnUpEvent();
		}
		if (connectedTool.IsBeingHeld() && isTriggerHeld)
		{
			HeldEvent?.Invoke();
		}

		if (previousState == true && !connectedTool.IsBeingHeld())
		{
			OnUpEvent();
		}

		previousState = connectedTool.IsBeingHeld();
		isTriggerHeldPreviousFrame = isTriggerHeld;
	}

	public void OnUpEvent()
    {
		Debug.Log("OnUpEvent");
		photonView.RPC(nameof(OnUpEventRPC), RpcTarget.Others);
		UpEvent?.Invoke();
    }

	[PunRPC]
	private void OnUpEventRPC(PhotonMessageInfo info)
    {
		UpEvent?.Invoke();
	}

	private void OnDownEvent()
    {
		Debug.Log("OnDownEvent");
		photonView.RPC(nameof(OnDownEventRPC), RpcTarget.Others);
		DownEvent?.Invoke();
	}

	[PunRPC]
	private void OnDownEventRPC(PhotonMessageInfo info)
    {
		DownEvent?.Invoke();
    }
}
