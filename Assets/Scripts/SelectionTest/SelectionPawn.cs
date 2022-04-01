using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionPawn : MonoBehaviour
{
	[SerializeField] private Transform headPos = null;
	[SerializeField] private Transform leftHandPos = null;
	[SerializeField] private Transform rightHandPos = null;
	[SerializeField] private float heightDifferenceForReady = 0.15f;


	public bool IsPawnGesturingReady(Hand hand = Hand.None)
	{
		if (hand == Hand.Left)
			return leftHandPos.position.y > headPos.position.y + heightDifferenceForReady;
		else if (hand == Hand.Right)
			return rightHandPos.position.y > headPos.position.y + heightDifferenceForReady;
		else
			return IsPawnGesturingReady(Hand.Right) || IsPawnGesturingReady(Hand.Left);
	}

	public bool IsPawnGesturingCheering(float heightDifferenceForReady)
	{
		return IsPawnGesturingReady(Hand.Right) && IsPawnGesturingReady(Hand.Left);
	}
}
