using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotSourceCollider : MonoBehaviour
{
	private void OnTriggerEnter(Collider other)
	{
		IRottable iRottable = other.GetComponentInChildren<IRottable>();
		if (iRottable == null)
			iRottable = other.GetComponentInParent<IRottable>();
		if (iRottable != null)
		{
			iRottable.SetIsRotting(true);
		}


	}

	private void OnTriggerExit(Collider other)
	{
		IRottable iRottable = other.GetComponentInChildren<IRottable>();
		if (iRottable == null)
			iRottable = other.GetComponentInParent<IRottable>();
		if (iRottable != null)
		{
			iRottable.SetIsRotting(false);
		}
	}
}
