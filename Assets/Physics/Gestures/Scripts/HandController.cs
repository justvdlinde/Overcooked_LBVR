using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PhysicsCharacter
{
	public enum FingerPoses
	{
		Neutral,
		Stretched,
		Folded,
		Null
	}

	public enum Fingers
	{
		Index,
		Middle,
		Ring,
		Pinky,
		Thumb,
		None
	}

	// This class controls controls the individual hands.
	public class HandController : MonoBehaviour
	{
		public Hand Hand { get { return hand; } }
		[SerializeField] private Hand hand = Hand.None;
		[SerializeField] private List<FingerController> fingers = new List<FingerController>();

		public Dictionary<Fingers, FingerPoseValuePair> fingerPoseDictionary { get; private set; } = new Dictionary<Fingers, FingerPoseValuePair>()
		{
			{ Fingers.Index, new FingerPoseValuePair(Fingers.Index, FingerPoses.Neutral, 0.0f) },
			{ Fingers.Middle, new FingerPoseValuePair(Fingers.Middle, FingerPoses.Neutral, 0.0f) },
			{ Fingers.Ring, new FingerPoseValuePair(Fingers.Ring, FingerPoses.Neutral, 0.0f) },
			{ Fingers.Pinky, new FingerPoseValuePair(Fingers.Pinky, FingerPoses.Neutral, 0.0f) },
			{ Fingers.Thumb, new FingerPoseValuePair(Fingers.Thumb, FingerPoses.Neutral, 0.0f) }
		};

		public float speed = 1.0f;

		private HandsDataDelegate handsDelegate = null;

		private void Start()
		{
			if(hand == Hand.Left)
				handsDelegate = HandsDataDelegate.LeftHandInstance;
			else if(hand == Hand.Right)
				handsDelegate = HandsDataDelegate.RightHandInstance;
		}

		// separate input from this class before porting.
		public void SetHandValues(FingerPoseValuePair index, FingerPoseValuePair middle, FingerPoseValuePair ring, FingerPoseValuePair pinky, FingerPoseValuePair thumb)
		{
			SetFingerPose(index);
			SetFingerPose(middle);
			SetFingerPose(ring);
			SetFingerPose(pinky);
			SetFingerPose(thumb);
		}

		private void SetFingerPose(FingerPoseValuePair pair)
		{
			fingerPoseDictionary[pair.finger] = pair;
			fingers[(int)pair.finger].SetFingerPose(pair.pose, pair.fingerValue);
		}

		// needs to follow controller which can be gotten via oculus call

		public struct FingerPoseValuePair
		{
			public Fingers finger;
			public FingerPoses pose;
			public float fingerValue;

			public FingerPoseValuePair(Fingers finger, FingerPoses pose, float fingerValue)
			{
				this.finger = finger;
				this.pose = pose;
				this.fingerValue = fingerValue;
			}
		}
	}
}
