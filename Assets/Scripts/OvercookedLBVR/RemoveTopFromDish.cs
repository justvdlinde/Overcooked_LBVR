using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PhysicsCharacter;

[RequireComponent(typeof(Rigidbody))]
public class RemoveTopFromDish : PickupableObject
{
	[SerializeField] private FoodStackSnapPoint dishSnapPoint = null;
	[SerializeField] private PickupableObject defaultToolHandle = null;
	private BoxCollider col = null;

	private void Awake()
	{
		rigidBody = GetComponent<Rigidbody>();
		col = GetComponent<BoxCollider>();
	}

	public override PickupableObject GetGrabbed(Hand hand, Transform target)
	{
		PickupableObject obj = dishSnapPoint.RemoveTopIngredient();
		//col.center = dishSnapPoint.GetComponent<BoxCollider>().center;
		if (obj != null)
			return obj.GetGrabbed(hand, target);
		else
			return defaultToolHandle.GetGrabbed(hand, target);
		//return otherToolHandle.GetGrabbed(hand, target);
	}

	public override void GetReleased(Hand hand)
	{
		return;
		//otherToolHandle.GetReleased(hand);
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