using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerPawnHandAnimatorRW : MonoBehaviourPun, IPunObservable
{
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

    private void Start()
    {
		// Because local and remote prefabs are different, the view IDs need to be setup manually:
		int increment = hand == Hand.Left ? PhotonIdentifiers.LeftHand : PhotonIdentifiers.RightHand;

		string str = rootPhotonView.Owner.ActorNumber.ToString() + increment.ToString();
		photonView.ViewID = int.Parse(str); ;
		photonView.TransferOwnership(rootPhotonView.Owner);
	}

    private void Update()
	{
		if(photonView.IsMine)
		{
			SetAnimatorValues();
		}
		else
		{
			float currentIndex = indexAnimator.GetFloat(animatorString);
			float currentMiddle = middleAnimator.GetFloat(animatorString);
			float currentRing = ringAnimator.GetFloat(animatorString);
			float currentPinky = pinkyAnimator.GetFloat(animatorString);
			float currentThumb = thumbAnimator.GetFloat(animatorString);

			float timeMul = Time.deltaTime * 3.5f;

			indexAnimator.SetFloat(animatorString, Mathf.MoveTowards(currentIndex, index, timeMul));
			middleAnimator.SetFloat(animatorString, Mathf.MoveTowards(currentIndex, middle, timeMul));
			ringAnimator.SetFloat(animatorString, Mathf.MoveTowards(currentIndex, middle, timeMul));
			pinkyAnimator.SetFloat(animatorString, Mathf.MoveTowards(currentIndex, middle, timeMul));
			thumbAnimator.SetFloat(animatorString, Mathf.MoveTowards(currentIndex, thumb, timeMul));
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