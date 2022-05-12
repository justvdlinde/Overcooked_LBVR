using PhysicsCharacter;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils.Core.Attributes;

public class RemotePawnHandSync : MonoBehaviour
{
	// order
	//	Neutral,
	//	FingerGun,
	//	Fist,
	//	IndexStretch,
	//	Thumbsup,
	//	OK
	[SerializeField] private List<GestureBase> gestures = new List<GestureBase>();
	[SerializeField] private GestureBase neutralGesture = null;

	[SerializeField] private Animator indexAnimator = null;
	[SerializeField] private Animator middleAnimator = null;
	[SerializeField] private Animator ringAnimator = null;
	[SerializeField] private Animator pinkyAnimator = null;
	[SerializeField] private Animator thumbAnimator = null;

	private GestureBase currentPose = null;

	private float indexSpeed = 1f;
	private float middleSpeed = 1f;
	private float ringSpeed = 1f;
	private float pinkySpeed = 1f;
	private float thumbSpeed = 1f;

	string animatorString = "FingerFloat";

	public HandPoses pose = HandPoses.Neutral;

	private void Awake()
	{
		currentPose = neutralGesture;

		indexAnimator.SetFloat(animatorString, currentPose.IndexPoseValue);
		middleAnimator.SetFloat(animatorString, currentPose.MiddlePoseValue);
		ringAnimator.SetFloat(animatorString, currentPose.RingPoseValue);
		pinkyAnimator.SetFloat(animatorString, currentPose.PinkyPoseValue);
		thumbAnimator.SetFloat(animatorString, currentPose.ThumbPoseValue);
	}

	[Button]
	public void setPose()
	{
		SetGesture(pose);
	}

	public void SetGesture(HandPoses pose)
	{
		int p = (int)pose;
		if (p > 0 && p < gestures.Count)
			currentPose = gestures[(int)pose];
		else
			currentPose = neutralGesture;

		float index = indexAnimator.GetFloat(animatorString);
		float middle = middleAnimator.GetFloat(animatorString);
		float ring = ringAnimator.GetFloat(animatorString);
		float pinky = pinkyAnimator.GetFloat(animatorString);
		float thumb = thumbAnimator.GetFloat(animatorString);

		float timeMul = Time.deltaTime * 3.5f;
		indexSpeed = GetDifference(index, GetAnimValue(Fingers.Index)) * timeMul;
		middleSpeed = GetDifference(middle, GetAnimValue(Fingers.Middle)) * timeMul;
		ringSpeed = GetDifference(ring, GetAnimValue(Fingers.Ring)) * timeMul;
		pinkySpeed = GetDifference(pinky, GetAnimValue(Fingers.Pinky)) * timeMul;
		thumbSpeed = GetDifference(thumb, GetAnimValue(Fingers.Thumb)) * timeMul;
	}

	private float GetAnimValue(Fingers finger)
	{
		switch (finger)
		{
			case Fingers.Index:
				return ((currentPose.IndexPose == FingerPoses.Stretched) ? -0.5f : currentPose.IndexPoseValue);
			case Fingers.Middle:
				return ((currentPose.MiddlePose == FingerPoses.Stretched) ? -0.5f : currentPose.MiddlePoseValue);
			case Fingers.Ring:
				return ((currentPose.RingPose == FingerPoses.Stretched) ? -0.5f : currentPose.RingPoseValue);
			case Fingers.Pinky:
				return ((currentPose.PinkyPose == FingerPoses.Stretched) ? -0.5f : currentPose.PinkyPoseValue);
			case Fingers.Thumb:
				return ((currentPose.ThumbPose == FingerPoses.Stretched) ? -0.5f : currentPose.ThumbPoseValue);
			default:
				return 0f;
		}
	}

	private void Update()
	{
		float index = indexAnimator.GetFloat(animatorString);
		float middle = middleAnimator.GetFloat(animatorString);
		float ring = ringAnimator.GetFloat(animatorString);
		float pinky = pinkyAnimator.GetFloat(animatorString);
		float thumb = thumbAnimator.GetFloat(animatorString);


		indexAnimator.SetFloat(animatorString, Mathf.MoveTowards(index, GetAnimValue(Fingers.Index), indexSpeed));
		middleAnimator.SetFloat(animatorString, Mathf.MoveTowards(middle, GetAnimValue(Fingers.Middle), middleSpeed));
		ringAnimator.SetFloat(animatorString, Mathf.MoveTowards(ring, GetAnimValue(Fingers.Ring), ringSpeed));
		pinkyAnimator.SetFloat(animatorString, Mathf.MoveTowards(pinky, GetAnimValue(Fingers.Pinky), pinkySpeed));
		thumbAnimator.SetFloat(animatorString, Mathf.MoveTowards(thumb, GetAnimValue(Fingers.Thumb), thumbSpeed));
	}

	private float GetDifference(float a, float b)
	{
		if (a > b)
			return a - b;
		return b - a;
	}
}
