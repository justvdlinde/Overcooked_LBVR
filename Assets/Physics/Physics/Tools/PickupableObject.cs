using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PhysicsCharacter
{
	[RequireComponent(typeof(Rigidbody))]
	public class PickupableObject : MonoBehaviour
	{
		// calculate offset from center pos and store
		// set RB to not use gravity
		// set RB to kinematic
		// set velocity to "parent" pos + offset calculated
		// set rotation to "parent" rotation compared to object rotation on pickup

		// TBD: disable physics layer interactions between set hand when grabbed
		// TBD: track velocity so it can be applied to object on release

		[Header("Right hand vars")]
		public bool allowRightGrip = true;
		// gesture for gripping right hand (set max for finger)

		[Header("Left hand vars")]
		public bool allowLeftGrip = true;
		// gesture for gripping left hand (set max for finger)

		protected Hand heldHand = Hand.None;
		protected bool isHeldLeftHand = false;
		protected bool isHeldRightHand = false;

		protected Rigidbody rigidBody = null;

		protected Transform parentObject = null;

		public virtual PickupableObject GetGrabbed(Hand hand, Transform target)
		{
			if(allowRightGrip && hand == Hand.Right && !isHeldRightHand)
			{
				if(isHeldLeftHand)
					GetReleased(Hand.Left);

				isHeldRightHand = true;
				heldHand = hand;

				parentObject = target;
				transform.parent = target;

				rigidBody.useGravity = false;
				rigidBody.isKinematic = true;
			}
			else if(allowLeftGrip && hand == Hand.Left && !isHeldLeftHand)
			{
				if(isHeldRightHand)
					GetReleased(Hand.Right);

				isHeldLeftHand = true;
				heldHand = hand;

				parentObject = target;
				transform.parent = target;

				rigidBody.useGravity = false;
				rigidBody.isKinematic = true;
			}
			return this;
		}

		public virtual void GetReleased(Hand hand)
		{
			if(hand == Hand.Right && isHeldRightHand)
			{
				transform.parent = null;
				isHeldRightHand = false;
				parentObject = null;
				heldHand = Hand.None;

				rigidBody.useGravity = true;
				rigidBody.isKinematic = false;

				// reset finger freeze values
				HandsDataDelegate.GetHandedInstance(hand).ResetHandPose();
			}

			if(hand == Hand.Left && isHeldLeftHand)
			{
				transform.parent = null;
				isHeldLeftHand = false;
				parentObject = null;
				heldHand = Hand.None;

				rigidBody.useGravity = true;
				rigidBody.isKinematic = false;

				// reset finger freeze values
				HandsDataDelegate.GetHandedInstance(hand).ResetHandPose();
			}
		}

		public virtual bool IsObjectBeingHeld()
		{
			return isHeldLeftHand || isHeldRightHand;
		}

		public virtual bool IsObjectBeingHeld(Hand hand)
		{
			if(hand == Hand.Right && isHeldRightHand)
			{
				return true;
			}
			else if(hand == Hand.Left && isHeldLeftHand)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		private void Awake()
		{
			rigidBody = GetComponent<Rigidbody>();
		}

	}

	public class TransformMirror
	{
		public Vector3 forward = Vector3.forward;
		public Vector3 up = Vector3.up;
		public Vector3 right = Vector3.right;
		public Vector3 position = Vector3.zero;
		public Quaternion rotation = Quaternion.identity;

		public TransformMirror(Transform t)
		{
			forward = t.forward;
			up = t.up;
			right = t.right;
			position = t.position;
			rotation = t.rotation;
		}
	}

	[System.Serializable]
	public class LocalTransformMirror
	{
		public Transform parent = null;
		public Vector3 forward = Vector3.forward;
		public Vector3 up = Vector3.up;
		public Vector3 right = Vector3.right;
		public Vector3 localPosition = Vector3.zero;
		public Quaternion localRotation = Quaternion.identity;

		public LocalTransformMirror(Transform t)
		{
			forward = t.forward;
			up = t.up;
			right = t.right;
			localPosition = t.localPosition;
			localRotation = t.localRotation;
			parent = t.parent;
		}

		public Vector3 GetUp()
		{
			return parent.TransformPoint(localPosition + Vector3.up);
		}

		public Vector3 GetForward()
		{
			return parent.TransformPoint(localPosition + Vector3.forward);
		}

		public Vector3 GetRight()
		{
			return parent.TransformPoint(localPosition + Vector3.right);
		}

		public Vector3 GetWorldPosition()
		{
			return parent.TransformPoint(localPosition);
		}

		public Quaternion GetWorldRotation()
		{
			return parent.rotation * localRotation;
		}
	}
}
