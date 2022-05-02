using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PhysicsCharacter;

[RequireComponent(typeof(Rigidbody))]
public class RemoveTopFromDishHandle : PickupableObject
{
	[SerializeField] private FoodStack foodStack = null;
	[SerializeField] private PickupableObject defaultToolHandle = null;
	private BoxCollider col = null;

	private void Awake()
	{
		rigidBody = GetComponent<Rigidbody>();
		col = GetComponent<BoxCollider>();
	}

	public override PickupableObject GetGrabbed(Hand hand, Transform target)
	{
		if (!foodStack.CanRemoveTopIngredient())
			return null;

		Ingredient ingredient = foodStack.RemoveTopIngredient();
        PickupableObject handle = ingredient.GrabController.dummyToolHandle;
		if (handle != null)
            return handle.GetGrabbed(hand, target);
        else
            return defaultToolHandle.GetGrabbed(hand, target);
	}

	public override void GetReleased(Hand hand)
	{
		return;
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