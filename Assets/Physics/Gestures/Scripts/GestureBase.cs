using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace PhysicsCharacter
{
	// Add to this enum when adding new gestures
	public enum HandPoses
	{
		Neutral,
		FingerGun,
		Fist,
		IndexStretch,
		Thumbsup,
		OK
	}

	[CreateAssetMenu]
	public class GestureBase : ScriptableObject
	{
		public HandPoses GestureHandPose { get => gestureHandPose; }
		[SerializeField] private HandPoses gestureHandPose = HandPoses.Neutral;

		[Header("Index finger values")]
		[SerializeField] private FingerPoses indexPose = FingerPoses.Neutral;
		[SerializeField, Range(0, 1f)] private float indexPoseValue = 0.0f;
		[SerializeField, Range(0, 1f)] private float indexDeadzone = 0.1f;

		[Header("Middle finger values")]
		[SerializeField] private FingerPoses middlePose = FingerPoses.Neutral;
		[SerializeField, Range(0, 1f)] private float middlePoseValue = 0.0f;
		[SerializeField, Range(0, 1f)] private float middleDeadzone = 0.1f;

		[Header("Ring finger values")]
		[SerializeField] private FingerPoses ringPose = FingerPoses.Neutral;
		[SerializeField, Range(0, 1f)] private float ringPoseValue = 0.0f;
		[SerializeField, Range(0, 1f)] private float ringDeadzone = 0.1f;

		[Header("Pinky finger values")]
		[SerializeField] private FingerPoses pinkyPose = FingerPoses.Neutral;
		[SerializeField, Range(0, 1f)] private float pinkyPoseValue = 0.0f;
		[SerializeField, Range(0, 1f)] private float pinkyDeadzone = 0.1f;

		[Header("Thumb values")]
		[SerializeField] private FingerPoses thumbPose = FingerPoses.Neutral;
		[SerializeField, Range(0, 1f)] private float thumbPoseValue = 0.0f;
		[SerializeField, Range(0, 1f)] private float thumbDeadzone = 0.1f;

		public float IndexPoseValue { get => indexPoseValue; }
		public float MiddlePoseValue { get => middlePoseValue; }
		public float RingPoseValue { get => ringPoseValue; }
		public float PinkyPoseValue { get => pinkyPoseValue; }
		public float ThumbPoseValue { get => thumbPoseValue; }


		public FingerPoses IndexPose { get => indexPose; }
		public FingerPoses MiddlePose { get => middlePose; }
		public FingerPoses RingPose { get => ringPose; }
		public FingerPoses PinkyPose { get => pinkyPose; }
		public FingerPoses ThumbPose { get => thumbPose; }

		public bool IsMatchingGesture(float index, float middle, float ring, float pinky, float thumb)
		{
			bool ind = (indexPose == FingerPoses.Folded) ? index >= indexPoseValue - indexDeadzone : index <= 0.0f + indexDeadzone;
			bool mid = (middlePose == FingerPoses.Folded) ? middle >= middlePoseValue - middleDeadzone : middle <= 0.0f + middleDeadzone;
			bool rin = (ringPose == FingerPoses.Folded) ? ring >= ringPoseValue - ringDeadzone : ring <= 0.0f + ringDeadzone;
			bool pin = (pinkyPose == FingerPoses.Folded) ? pinky >= pinkyPoseValue - pinkyDeadzone : pinky <= 0.0f + pinkyDeadzone;
			bool thum = (thumbPose == FingerPoses.Folded) ? thumb >= thumbPoseValue - thumbDeadzone : thumb <= 0.0f + thumbDeadzone;

			return ind && mid && rin && pin && thum;
		}
	}
}
