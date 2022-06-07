using Photon.Pun;
using PhysicsCharacter;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPawnHandAnimatorRW : MonoBehaviourPun, IPunObservable
{
	private const float lerpSpeed = 10f;

	[SerializeField] private PhotonView rootPhotonView = null;
	[SerializeField] private Hand hand = Hand.None;

	[SerializeField] private Animator indexAnimator = null;
	[SerializeField] private Animator middleAnimator = null;
	[SerializeField] private Animator ringAnimator = null;
	[SerializeField] private Animator pinkyAnimator = null;
	[SerializeField] private Animator thumbAnimator = null;

	private float index = 0;
	private float middle = 0;
	private float ring = 0;
	private float pinky = 0;
	private float thumb = 0;

	private const string animatorString = "FingerFloat";

	[SerializeField] private PhysicsPickupManager pickupManager = null;

	[Header("remote only vars")]
	[SerializeField] private Transform handGraphics = null;
	[SerializeField] private Transform pickupPivot = null;
	private Vector3 originalLocalPos;
	private Quaternion originalLocalRot;
	private Vector3 originalLocalScale;

	public bool isRemoteClient = false;
	private Vector3 followTarget;

	private void Awake()
	{
		if(isRemoteClient && handGraphics != null)
		{
			originalLocalPos = handGraphics.localPosition;
			originalLocalRot = handGraphics.localRotation;
			originalLocalScale = handGraphics.localScale;
		}
	}
	
	private void OnEnable()
	{
		if(!isRemoteClient && pickupManager != null)
		{
			pickupManager.OnPickupEvent += OnPickupEvent;
			pickupManager.OnDropEvent += OnDropEvent;
		}
	}

	private void OnDisable()
	{
		if(!isRemoteClient && pickupManager != null)
		{
			pickupManager.OnPickupEvent -= OnPickupEvent;
			pickupManager.OnDropEvent -= OnDropEvent;
		}
	}

	private void OnPickupEvent(Hand hand, Tool tool)
	{
		//Debug.Log($"picked up photonview ID {tool.RootPhotonView.ViewID} with {hand.ToString()} hand");
		photonView.RPC(nameof(OnPickupEventRPC), RpcTarget.Others, tool.RootPhotonView.ViewID);

	}

	[PunRPC]
	public void OnPickupEventRPC(int viewID)
	{
		Tool tool = PhotonView.Find(viewID).GetComponentInChildren<Tool>();

		if (!isRemoteClient)
			return;

		ToolHandle t = tool.GetTargToolhandle(hand);

		heldToolhandle = t;

		handGraphics.transform.parent = null;
		handGraphics.transform.position = t.transform.position;
		handGraphics.transform.rotation = t.transform.rotation;

		// get offset from somewhere
		Vector3 offset = new Vector3((hand == Hand.Left) ? -0.024f : 0.024f, -0.002f, -0.13f);
		if (t.UseHandleHandedness)
			offset = Vector3.zero;

		handGraphics.transform.position = t.localTransformMirror.GetWorldPosition();
		handGraphics.transform.localPosition += offset;
		float zOffset = (hand == Hand.Left) ? 90f : -90f;
		handGraphics.rotation = t.localTransformMirror.GetWorldRotation() * Quaternion.Euler(new Vector3(0,0,zOffset));

		if(t.UseHandleHandedness)
		{
			Vector3 diffToHandle = pickupPivot.position - t.transform.position;
			handGraphics.transform.localPosition -= diffToHandle;
		}
	}


	private void OnDropEvent(Hand hand, Tool tool)
	{
		photonView.RPC(nameof(OnDropEventRPC), RpcTarget.Others, tool.RootPhotonView.ViewID);
	}

	[PunRPC]
	public void OnDropEventRPC(int viewID)
	{
		Tool tool = PhotonView.Find(viewID).GetComponentInChildren<Tool>();

		if (!isRemoteClient)
			return;

		heldToolhandle = null;

		handGraphics.transform.parent = transform;
		followTarget = transform.position;
		handGraphics.localPosition = originalLocalPos;
		handGraphics.localRotation = originalLocalRot;
		handGraphics.localScale = originalLocalScale;
	}

	[SerializeField] private Transform defaultPosition = null;
	private ToolHandle heldToolhandle = null;

    private void Update()
	{
		if(photonView.IsMine)
		{
			SetAnimatorValues();
		}
		else
		{
			if(isRemoteClient)
			{
				if (heldToolhandle != null)
				{
					// get offset from somewhere
					Vector3 offset = new Vector3((hand == Hand.Left) ? -0.024f : 0.024f, -0.002f, -0.13f);
					if (heldToolhandle.UseHandleHandedness)
						offset = Vector3.zero;

					handGraphics.transform.position = heldToolhandle.localTransformMirror.GetWorldPosition();
					handGraphics.transform.localPosition += offset;
					float zOffset = (hand == Hand.Left) ? 90f : -90f;
					handGraphics.rotation = heldToolhandle.localTransformMirror.GetWorldRotation() * Quaternion.Euler(new Vector3(0, 0, zOffset));

					if (heldToolhandle.UseHandleHandedness)
					{
						Vector3 diffToHandle = pickupPivot.position - heldToolhandle.transform.position;
						handGraphics.transform.localPosition -= diffToHandle;
					}
				}
				else
				{
					handGraphics.transform.parent = transform;
					handGraphics.transform.position = defaultPosition.position;
					handGraphics.transform.rotation = defaultPosition.rotation;
				}
			}

			float timeMul = Time.deltaTime * lerpSpeed;

			if (indexAnimator != null)
			{
				float currentIndex = indexAnimator.GetFloat(animatorString);
				indexAnimator.SetFloat(animatorString, Mathf.MoveTowards(currentIndex, index, timeMul));
			}
			if (indexAnimator != null)
			{
				float currentMiddle = middleAnimator.GetFloat(animatorString);
				middleAnimator.SetFloat(animatorString, Mathf.MoveTowards(currentMiddle, middle, timeMul));
			}
			if (indexAnimator != null)
			{
				float currentRing = ringAnimator.GetFloat(animatorString);
				ringAnimator.SetFloat(animatorString, Mathf.MoveTowards(currentRing, middle, timeMul));
			}
			if (indexAnimator != null)
			{
				float currentPinky = pinkyAnimator.GetFloat(animatorString);
				pinkyAnimator.SetFloat(animatorString, Mathf.MoveTowards(currentPinky, middle, timeMul));
			}
			if (indexAnimator != null)
			{
				float currentThumb = thumbAnimator.GetFloat(animatorString);
				thumbAnimator.SetFloat(animatorString, Mathf.MoveTowards(currentThumb, thumb, timeMul));
			}
		}
	}

	private void SetAnimatorValues()
	{
		index = indexAnimator.GetFloat(animatorString);
		middle = middleAnimator.GetFloat(animatorString);
		ring = ringAnimator.GetFloat(animatorString);
		pinky = pinkyAnimator.GetFloat(animatorString);
		thumb = thumbAnimator.GetFloat(animatorString);
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if(photonView.IsMine)
		{
			stream.SendNext(index);
			stream.SendNext(middle);
			// uncomment if index controller
			//stream.SendNext(ring);
			//stream.SendNext(pinky);
			stream.SendNext(thumb);
		}
		else
		{
			index = (float)stream.ReceiveNext();
			middle = (float)stream.ReceiveNext();
			// individual read if index
			ring = middle;
			pinky = middle;
			//ring = (float)stream.ReceiveNext();
			//pinky = (float)stream.ReceiveNext();

			thumb = (float)stream.ReceiveNext();
		}
	}
}