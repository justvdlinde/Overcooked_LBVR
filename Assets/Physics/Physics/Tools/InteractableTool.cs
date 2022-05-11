using Photon.Pun;
using PhysicsCharacter;
using UnityEngine;
using UnityEngine.Events;

public class InteractableTool : MonoBehaviourPun
{
    [SerializeField] private Tool connectedTool = null;
	[SerializeField] private UnityEvent UpEvent = null;
	[SerializeField] private UnityEvent HeldEvent = null;
	[SerializeField] private UnityEvent DownEvent = null;

	private bool previousState = false;
	private bool isTriggerHeld = false;
	private bool isTriggerHeldPreviousFrame = false;

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
		photonView.RPC(nameof(OnUpEventRPC), RpcTarget.Others);
		OnUpEventInternal();
    }

	[PunRPC]
	protected void OnUpEventRPC(PhotonMessageInfo info)
    {
		OnUpEventInternal();
	}

	protected virtual void OnUpEventInternal()
    {
		UpEvent?.Invoke();
    }

	private void OnDownEvent()
    {
		photonView.RPC(nameof(OnDownEventRPC), RpcTarget.Others);
		OnDownEventInternal();
	}

	[PunRPC]
	protected void OnDownEventRPC(PhotonMessageInfo info)
    {
		OnDownEventInternal();
    }

	protected virtual void OnDownEventInternal()
    {
		DownEvent?.Invoke();
    }
}
