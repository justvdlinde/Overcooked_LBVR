using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PhysicsCharacter
{
	public enum ToolHandlePriority
	{
		Lowest,
		Lower,
		Low,
		Neutral,
		High,
		Higher,
		Highest
	}

	[RequireComponent(typeof(Rigidbody))]
	public class ToolHandle : PickupableObject
	{
		public bool HandleIsCloseToToolPos = false;
		[SerializeField] private float throwDistance = 0.25f;
		private Collider col = null;


		public Action<Hand, ToolHandle> OnGrabbedCall = null;
		public Action<Hand, ToolHandle> OnReleasedCall = null;

		public LocalTransformMirror localTransformMirror = null;
		public Vector3 localPosOffset { get; private set; } = Vector3.zero;
		public Quaternion localRotOffset { get; private set; } = Quaternion.identity;
		[SerializeField] private float reEnableDistance = .60f;
		[Header("Handle settings")]
		[SerializeField] private ToolHandlePriority handlePriority = ToolHandlePriority.Neutral;
		[SerializeField] private ToolHandleHandPose handlePose = null;

		[Header("Handed Tool Handle Settings")]
		[SerializeField] private bool useHandleHandedness = false;
		[SerializeField] public Hand preferredToolHand = Hand.None;
		[SerializeField] private ToolHandle otherToolHandle = null;
		public ToolHandle OtherToolHandle => otherToolHandle;

		private Transform oldParentLeft = null;
		private Transform oldParentRight = null;

		public bool releasedLeftHand { get; private set; } = false;
		public bool releasedRightHand { get; private set; } = false;

		public bool isGrabbedByRemote;

		private void Awake()
		{
			col = GetComponent<Collider>();
			rigidBody = GetComponent<Rigidbody>();
			localTransformMirror = new LocalTransformMirror(transform);
			localPosOffset = transform.parent.position - transform.position;
			localRotOffset = transform.parent.rotation * Quaternion.Inverse(transform.rotation);
		}

		private void Update()
		{
			if (checkDistanceOnReleaseLeft)
			{
				if (Vector3.Distance(PhysicsPlayerBlackboard.Instance.leftController.position, transform.position) > throwDistance)
				{
					PhysicsPlayerBlackboard.Instance.SetToolAndHandCollisions(Hand.Left, false);
					checkDistanceOnReleaseLeft = false;
				}
			}
			if (checkDistanceOnReleaseRight)
			{
				if (Vector3.Distance(PhysicsPlayerBlackboard.Instance.rightController.position, transform.position) > throwDistance)
				{
					PhysicsPlayerBlackboard.Instance.SetToolAndHandCollisions(Hand.Right, false);
					checkDistanceOnReleaseRight = false;
				}
			}

			// excape this entire section when the tool handles are not biased
			if (otherToolHandle == null)
				return;

			if(releasedLeftHand && oldParentLeft != null)
			{
				// check distance from left hand and remove layer ignoring on leaving range
				if(!otherToolHandle.isHeldLeftHand && Vector3.Distance(oldParentLeft.position, transform.position) >= reEnableDistance)
				{
					oldParentLeft = null;
					releasedLeftHand = false;
					PhysicsPlayerBlackboard.Instance.SetToolAndHandCollisions(Hand.Left, false);
					//Physics.IgnoreLayerCollision(KHS_PhysicsCharacter.KHS_PhysicsPlayerBlackboard.TOOL_LAYER_MASK, KHS_PhysicsCharacter.KHS_PhysicsPlayerBlackboard.LEFT_HAND_LAYER_MASK, false);
				}

				if(otherToolHandle.isHeldLeftHand)
				{
					oldParentLeft = null;
					releasedLeftHand = false;
				}
			}
			if(releasedRightHand && oldParentRight != null)
			{
				// check distance from right hand and remove layer ignoring on leaving range
				if(!otherToolHandle.isHeldRightHand && Vector3.Distance(oldParentRight.position, transform.position) >= reEnableDistance)
				{
					oldParentRight = null;
					releasedRightHand = false;
					PhysicsPlayerBlackboard.Instance.SetToolAndHandCollisions(Hand.Right, false);
					//Physics.IgnoreLayerCollision(KHS_PhysicsCharacter.KHS_PhysicsPlayerBlackboard.TOOL_LAYER_MASK, KHS_PhysicsCharacter.KHS_PhysicsPlayerBlackboard.RIGHT_HAND_LAYER_MASK, false);
				}

				if(otherToolHandle.isHeldRightHand)
				{
					oldParentRight = null;
					releasedRightHand = false;
				}
			}
		}

		public void CheckDistance(float maxGripDist)
		{
			if(IsObjectBeingHeld() && !PhysicsPlayerBlackboard.Instance.isFading)
			{
				if(Vector3.Distance(localTransformMirror.GetWorldPosition(), transform.position) > maxGripDist)
				{
					Debug.Log("Release call");
					GetReleased(heldHand);
				}
			}
		}

		public override PickupableObject GetGrabbed(Hand hand, Transform target)
		{
			if (hand == Hand.None)
				return otherToolHandle.GetGrabbed(hand, target);

			if (allowRightGrip && hand == Hand.Right && !isHeldRightHand)
			{
				if(useHandleHandedness)
				{
					if(hand != preferredToolHand)
					{
						if(otherToolHandle.preferredToolHand == hand)
						{
							return otherToolHandle.GetGrabbed(hand, target);
						}
					}
				}

				isHeldRightHand = true;

				if(isHeldLeftHand)
					GetReleased(Hand.Left);

				heldHand = hand;

				PhysicsPlayerBlackboard.Instance.SetToolAndHandCollisions(Hand.Right, true);

				SetHandData(hand, target);

				OnGrabbedCall?.Invoke(hand, this);

				parentObject = target;
				transform.parent = target;
				transform.localPosition = Vector3.zero;
				transform.localRotation = Quaternion.identity;

			}
			else if(allowLeftGrip && hand == Hand.Left && !isHeldLeftHand)
			{
				if(useHandleHandedness)
				{
					if(hand != preferredToolHand)
					{
						if(otherToolHandle.preferredToolHand == hand)
							return otherToolHandle.GetGrabbed(hand, target);
					}
				}

				isHeldLeftHand = true;
				if(isHeldRightHand)
					GetReleased(Hand.Right);

				heldHand = hand;

				PhysicsPlayerBlackboard.Instance.SetToolAndHandCollisions(Hand.Left, true);

				SetHandData(hand, target);

				OnGrabbedCall?.Invoke(hand, this);
				
				parentObject = target;
				transform.parent = target;
				transform.localPosition = Vector3.zero;
				transform.localRotation = Quaternion.identity;

			}

			return this;
		}

		public override void GetReleased(Hand hand)
		{
			if(hand == Hand.Right && isHeldRightHand)
			{
				oldParentRight = transform.parent;
				releasedRightHand = true;

				transform.parent = null;

				isHeldRightHand = false;
				parentObject = null;
				heldHand = Hand.None;


				ResetHandData(hand);
				OnReleasedCall?.Invoke(hand, this);
			}
			if(hand == Hand.Left && isHeldLeftHand)
			{
				oldParentLeft = transform.parent;
				releasedLeftHand = true;

				transform.parent = null;
				isHeldLeftHand = false;
				parentObject = null;
				heldHand = Hand.None;

				ResetHandData(hand);
				OnReleasedCall?.Invoke(hand, this);
			}
		}

		public void ForceRelease()
		{
			transform.parent = null;
			isHeldLeftHand = false;
			parentObject = null;
			heldHand = Hand.None;

			ResetHandData(Hand.Left);
			ResetHandData(Hand.Right);

			PhysicsPlayerBlackboard.Instance.DropItem(Hand.Left);
			PhysicsPlayerBlackboard.Instance.DropItem(Hand.Right);

			PhysicsPlayerBlackboard.Instance.SetToolAndHandCollisions(Hand.Left, false);
			PhysicsPlayerBlackboard.Instance.SetToolAndHandCollisions(Hand.Right, false);

			releasedLeftHand = false;
			releasedRightHand = false;

			OnReleasedCall?.Invoke(Hand.Left, this);
			OnReleasedCall?.Invoke(Hand.Right, this);
		}

		public override bool IsObjectBeingHeld()
		{
			if (isGrabbedByRemote)
				return true;

			if (useHandleHandedness)
				return heldHand == preferredToolHand;
			else
				return base.IsObjectBeingHeld();
		}

		public override bool IsObjectBeingHeld(Hand hand)
		{
			if (isGrabbedByRemote)
				return true;

			if(useHandleHandedness)
				return hand == preferredToolHand && heldHand == preferredToolHand;
			else
				return base.IsObjectBeingHeld(hand);
		}

		public ToolHandlePriority GetHandlePriority()
		{
			return handlePriority;
		}

		private void SetHandData(Hand hand, Transform target)
		{
			HandsDataDelegate handDelegate = HandsDataDelegate.GetHandedInstance(hand);
			handDelegate.SetIsHoldingObject(true);
			handDelegate.HandTargetToolhandle = this;
			handDelegate.SetFingerMax(Fingers.Index, handlePose.IndexPose, handlePose.IndexPoseValue);
			handDelegate.SetFingerMax(Fingers.Middle, handlePose.MiddlePose, handlePose.MiddlePoseValue);
			handDelegate.SetFingerMax(Fingers.Ring, handlePose.RingPose, handlePose.RingPoseValue);
			handDelegate.SetFingerMax(Fingers.Pinky, handlePose.PinkyPose, handlePose.PinkyPoseValue);
			handDelegate.SetFingerMax(Fingers.Thumb, handlePose.ThumbPose, handlePose.ThumbPoseValue);
		}

		private bool checkDistanceOnReleaseRight = false;
		private bool checkDistanceOnReleaseLeft = false;


		private void ResetHandData(Hand hand)
		{
			HandsDataDelegate handDelegate = HandsDataDelegate.GetHandedInstance(hand);
			handDelegate.SetIsHoldingObject(false);
			handDelegate.ResetHandPose();
			handDelegate.HandTargetToolhandle = null;


			if (hand == Hand.Left)
				checkDistanceOnReleaseLeft = true;
			else
				checkDistanceOnReleaseRight = true;
		}
	}

	[System.Serializable]
	public class ToolHandleHandPose
	{
		[Header("Index finger values")]
		[SerializeField] private FingerPoses indexPose = FingerPoses.Folded;
		[SerializeField, Range(0, 1f)] private float indexPoseValue = 1.0f;

		[Header("Middle finger values")]
		[SerializeField] private FingerPoses middlePose = FingerPoses.Folded;
		[SerializeField, Range(0, 1f)] private float middlePoseValue = 1.0f;

		[Header("Ring finger values")]
		[SerializeField] private FingerPoses ringPose = FingerPoses.Folded;
		[SerializeField, Range(0, 1f)] private float ringPoseValue = 1.0f;

		[Header("Pinky finger values")]
		[SerializeField] private FingerPoses pinkyPose = FingerPoses.Folded;
		[SerializeField, Range(0, 1f)] private float pinkyPoseValue = 1.0f;

		[Header("Thumb values")]
		[SerializeField] private FingerPoses thumbPose = FingerPoses.Folded;
		[SerializeField, Range(0, 1f)] private float thumbPoseValue = 1.0f;

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
	}
}
