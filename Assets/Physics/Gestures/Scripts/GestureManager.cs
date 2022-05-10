using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PhysicsCharacter
{
	public class GestureManager : MonoBehaviour
	{
		[SerializeField] private List<GestureBase> gestures = new List<GestureBase>();

		// Check input here
		// Check if fingers are not physics blocked
		// Check against gestures to see if any are valid
		// Apply valid gesture/finger stretch

		[SerializeField] private HandController handController = null;
		public HandPoses currentGesture { get; private set; } = HandPoses.Neutral;

		public void SetCurrentGesture(HandPoses pose)
		{
			currentGesture = pose;
		}

		public void Update()
		{
			
			float index = XRInput.GetTriggerButtonValue(handController.Hand);

			float middle = XRInput.GetGripButtonValue(handController.Hand);
			// mirror middle finger because touch had no seperate fingers
			float ring = XRInput.GetGripButtonValue(handController.Hand);
			// mirror middle finger because touch had no seperate fingers
			float pinky = XRInput.GetGripButtonValue(handController.Hand);

			float thumb = handController.fingerPoseDictionary[Fingers.Thumb].fingerValue;

			thumb = Mathf.Clamp01(thumb);
			if(XRInput.ThumbStickButtonIsTouched(handController.Hand) && thumb < 1f)
				thumb += 0.15f;
			else if(!XRInput.ThumbStickButtonIsTouched(handController.Hand) && thumb > 0f)
				thumb -= 0.15f;
			thumb = Mathf.Clamp01(thumb);

			// check for gestures
			GestureBase gesture = null;
			if(CompareGesture(out gesture, index, middle, ring, pinky, thumb))
			{
				float indexValue = index;
				float middleValue = middle;
				float ringValue = ring;
				float pinkyValue = pinky;
				float thumbValue = thumb;
				handController.SetHandValues(
					new HandController.FingerPoseValuePair(Fingers.Index, gesture.IndexPose, indexValue),
					new HandController.FingerPoseValuePair(Fingers.Middle, gesture.MiddlePose, middleValue),
					new HandController.FingerPoseValuePair(Fingers.Ring, gesture.RingPose, ringValue),
					new HandController.FingerPoseValuePair(Fingers.Pinky, gesture.PinkyPose, pinkyValue),
					new HandController.FingerPoseValuePair(Fingers.Thumb, gesture.ThumbPose, thumbValue)
					);

				SetCurrentGesture(gesture.GestureHandPose);
			}
			else
			{
				handController.SetHandValues(
					new HandController.FingerPoseValuePair(Fingers.Index, FingerPoses.Null, index),
					new HandController.FingerPoseValuePair(Fingers.Middle, FingerPoses.Null, middle),
					new HandController.FingerPoseValuePair(Fingers.Ring, FingerPoses.Null, ring),
					new HandController.FingerPoseValuePair(Fingers.Pinky, FingerPoses.Null, pinky),
					new HandController.FingerPoseValuePair(Fingers.Thumb, FingerPoses.Null, thumb)
					);
				SetCurrentGesture(HandPoses.Neutral);
			}
		}

		private bool CompareGesture(out GestureBase gesture, float index, float middle, float ring, float pinky, float thumb)
		{
			bool metGesture = false;
			GestureBase matchedGesture = null;

			foreach(GestureBase g in gestures)
			{
				if(g.IsMatchingGesture(index, middle, ring, pinky, thumb))
				{
					// gesture found
					matchedGesture = g;
					metGesture = true;
					break;
				}
			}

			gesture = matchedGesture;
			return metGesture;
		}
	}
}
