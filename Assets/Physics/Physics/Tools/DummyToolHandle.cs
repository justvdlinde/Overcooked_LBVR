using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PhysicsCharacter
{
	[RequireComponent(typeof(Rigidbody))]
	public class DummyToolHandle : PickupableObject
	{
		[Header("Handed Tool Handle Settings")]
		[SerializeField] private bool useHandleHandedness = false;
		[SerializeField] public Hand preferredToolHand = Hand.None;
		[SerializeField] private ToolHandle otherToolHandle = null;
		public ToolHandle OtherToolHandle => otherToolHandle;

		public bool releasedLeftHand { get; private set; } = false;
		public bool releasedRightHand { get; private set; } = false;

		private void Awake()
		{
			rigidBody = GetComponent<Rigidbody>();
		}

		public override PickupableObject GetGrabbed(Hand hand, Transform target)
		{
			return otherToolHandle.GetGrabbed(hand, target);
		}

		public override void GetReleased(Hand hand)
		{
			otherToolHandle.GetReleased(hand);
		}

		public override bool IsObjectBeingHeld()
		{
			return false;
		}

		public override bool IsObjectBeingHeld(Hand hand)
		{
			return false;
		}
	}
}
