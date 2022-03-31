using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionPawn : MonoBehaviour
{
	[SerializeField] private Transform headPos = null;
	[SerializeField] private Transform leftHandPos = null;
	[SerializeField] private Transform rightHandPos = null;

	public bool IsPawnGesturingReady(float heightDifferenceForReady, Hand hand = Hand.None)
	{
		if (hand == Hand.Left)
			return leftHandPos.position.y > headPos.position.y + heightDifferenceForReady;
		else if (hand == Hand.Right)
			return rightHandPos.position.y > headPos.position.y + heightDifferenceForReady;
		else
			return IsPawnGesturingReady(heightDifferenceForReady, Hand.Right) || IsPawnGesturingReady(heightDifferenceForReady, Hand.Left);
	}

	public bool IsPawnGesturingCheering(float heightDifferenceForReady)
	{
		return IsPawnGesturingReady(heightDifferenceForReady, Hand.Right) && IsPawnGesturingReady(heightDifferenceForReady, Hand.Left);
	}
}
