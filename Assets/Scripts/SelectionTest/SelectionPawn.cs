using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelectionPawn : MonoBehaviour
{
	[SerializeField] private Transform headPos = null;
	[SerializeField] private Transform leftHandPos = null;
	[SerializeField] private Transform rightHandPos = null;
	[SerializeField] private float heightDifferenceForReady = 0.15f;

	[SerializeField] private GameObject infoUI = null;
	[SerializeField] private TMPro.TextMeshProUGUI displayText = null;

	private SelectionManager selectionManager = null;

	private void GetSelectionManager()
	{
		selectionManager = GameObject.FindObjectOfType<SelectionManager>();
	}

	private void Update()
	{
		if (!SelectionManager.IsSelectionActive)
		{
			infoUI.SetActive(false);
			return;
		}
		else
			infoUI.SetActive(true);

		if (selectionManager == null)
			GetSelectionManager();

		string text = selectionManager.GetTextForSelectionType(selectionManager.selectionType) + "\n";

		if (!selectionManager.IsPawnInSelectionVolume())
		{
			text += "Move to a selection area to be able to vote";
		}
		else
		{
			if (SelectionManager.IsPawnReady)
				text += "Ready! Waiting for others to vote";
			else
				text += "Raise a hand to signal your vote";
		}

		displayText.text = text;
	}


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
