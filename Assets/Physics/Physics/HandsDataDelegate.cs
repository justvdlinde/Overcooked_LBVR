using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PhysicsCharacter
{
	public class HandsDataDelegate : MonoBehaviour
	{
		public static HandsDataDelegate RightHandInstance = null;
		public static HandsDataDelegate LeftHandInstance = null;

		public static HandsDataDelegate GetHandedInstance(Hand hand)
		{
			if(hand == Hand.Right)
				return RightHandInstance;
			else if(hand == Hand.Left)
				return LeftHandInstance;
			else
				return null;
		}

		[SerializeField] private Hand hand = Hand.None;
		[SerializeField] private Transform handFollowTarget = null;

		// these values are only set by tool handles.
		// any other freezing happens in the finger managers individually.
		private FingerPoses indexPose = FingerPoses.Neutral;
		private float indexMax = 1.0f;

		private FingerPoses middlePose = FingerPoses.Neutral;
		private float middleMax = 1.0f;

		private FingerPoses ringPose = FingerPoses.Neutral;
		private float ringMax = 1.0f;

		private FingerPoses pinkyPose = FingerPoses.Neutral;
		private float pinkyMax = 1.0f;

		private FingerPoses thumbPose = FingerPoses.Neutral;
		private float thumbMax = 1.0f;

		public bool IsHoldingObject { get; private set; } = false;

		public ToolHandle HandTargetToolhandle = null;

		[SerializeField] private float maxHandDistance = 0.5f;

		public bool IsHandInRange()
		{
			if(HandTargetToolhandle != null)
			{
				float d = Vector3.Distance(handFollowTarget.transform.position, HandTargetToolhandle.localTransformMirror.GetWorldPosition());
				return d <= maxHandDistance;
			}
			else
				return true;
		}

		private void Awake()
		{
			if(hand == Hand.Left && LeftHandInstance == null)
				LeftHandInstance = this;
			else if(hand == Hand.Right && RightHandInstance == null)
				RightHandInstance = this;
			else
				Debug.LogError("Failed to set hand manager");
		}

		public float GetFingerValue(Fingers finger)
		{
			switch(finger)
			{
				case Fingers.Index:
					return indexMax;
				case Fingers.Middle:
					return middleMax;
				case Fingers.Ring:
					return ringMax;
				case Fingers.Pinky:
					return pinkyMax;
				case Fingers.Thumb:
					return thumbMax;
				case Fingers.None:
					return 1.0f;
				default:
					return 1.0f;
			}
		}

		public void SetFingerMax(Fingers finger, FingerPoses pose, float value)
		{
			switch(finger)
			{
				case Fingers.Index:
					indexMax = Mathf.Clamp01(value);
					indexPose = pose;
					break;
				case Fingers.Middle:
					middleMax = Mathf.Clamp01(value);
					middlePose = pose;
					break;
				case Fingers.Ring:
					ringMax = Mathf.Clamp01(value);
					ringPose = pose;
					break;
				case Fingers.Pinky:
					pinkyMax = Mathf.Clamp01(value);
					pinkyPose = pose;
					break;
				case Fingers.Thumb:
					thumbMax = Mathf.Clamp01(value);
					thumbPose = pose;
					break;
				case Fingers.None:
					return;
				default:
					return;
			}
		}

		public void ResetFinger(Fingers finger)
		{
			switch(finger)
			{
				case Fingers.Index:
					indexMax = 1.0f;
					indexPose = FingerPoses.Neutral;
					break;
				case Fingers.Middle:
					middleMax = 1.0f;
					middlePose = FingerPoses.Neutral;
					break;
				case Fingers.Ring:
					ringMax = 1.0f;
					ringPose = FingerPoses.Neutral;
					break;
				case Fingers.Pinky:
					pinkyMax = 1.0f;
					pinkyPose = FingerPoses.Neutral;
					break;
				case Fingers.Thumb:
					thumbMax = 1.0f;
					thumbPose = FingerPoses.Neutral;
					break;
				case Fingers.None:
					return;
				default:
					return;
			}
		}

		// full reset on hand
		public void ResetHandPose()
		{
			indexMax = 1.0f;
			indexPose = FingerPoses.Neutral;
			middleMax = 1.0f;
			middlePose = FingerPoses.Neutral;
			ringMax = 1.0f;
			ringPose = FingerPoses.Neutral;
			pinkyMax = 1.0f;
			pinkyPose = FingerPoses.Neutral;
			thumbMax = 1.0f;
			thumbPose = FingerPoses.Neutral;
			SetIsHoldingObject(false);
		}

		public void SetIsHoldingObject(bool val)
		{
			IsHoldingObject = val;
		}

		public Hand GetHand()
		{
			return hand;
		}

		public Vector3 GetFollowPosition()
		{
			//if(IsHoldingObject && HandTargetToolhandle != null)
			//	return HandTargetToolhandle.localTransformMirror.GetWorldPosition();
			return GetControllerPosition();
		}

		public Vector3 GetControllerPosition()
		{
			return handFollowTarget.position;
		}

		public Quaternion GetFollowRotation()
		{
			//if(IsHoldingObject && HandTargetToolhandle != null)
			//	return HandTargetToolhandle.localTransformMirror.GetWorldRotation();
			return handFollowTarget.rotation;
		}

		public Transform GetFollowTarget()
		{
			return handFollowTarget;
		}
	}
}