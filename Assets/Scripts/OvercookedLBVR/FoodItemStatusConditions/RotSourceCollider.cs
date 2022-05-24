using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotSourceCollider : MonoBehaviour
{
	private void OnTriggerEnter(Collider other)
	{
		IngredientStatusCondition statusCondition = other.GetComponentInChildren<IngredientStatusCondition>();
		if (statusCondition == null)
			statusCondition = other.GetComponentInParent<IngredientStatusCondition>();
		if (statusCondition != null)
		{
			statusCondition.SetIsRotting(true);
		}
	}

	private void OnTriggerExit(Collider other)
	{
		IngredientStatusCondition statusCondition = other.GetComponentInChildren<IngredientStatusCondition>();
		if (statusCondition == null)
			statusCondition = other.GetComponentInParent<IngredientStatusCondition>();
		if (statusCondition != null)
		{
			statusCondition.SetIsRotting(false);
		}
	}
}
