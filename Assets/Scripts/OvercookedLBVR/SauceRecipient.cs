using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SauceRecipient : MonoBehaviour
{
	[SerializeField] private float timeUntillSaucePlaces = 3;
	private float currentProgressTillKetchup = 0;
	private float currentProgressTillMayo = 0;
	[SerializeField] private GameObject ketchupPrefab = null;
	[SerializeField] private GameObject mayoPrefab = null;
	private bool hasTriggered = false;

    public void ProgressSauceValue(float value, IngredientType sauceType)
	{
		if (hasTriggered)
			return;
		if (sauceType == IngredientType.Ketchup)
			currentProgressTillKetchup += value;
		else
			currentProgressTillMayo += value;

		Debug.Log("placing sauce " + sauceType.ToString() + " mayo progress " + currentProgressTillMayo + " ketchup progress " + currentProgressTillKetchup);


		if (currentProgressTillKetchup >= timeUntillSaucePlaces)
			ApplySauce(IngredientType.Ketchup);
		else if(currentProgressTillMayo >= timeUntillSaucePlaces)
			ApplySauce(IngredientType.Mayo);
	}

	private void ApplySauce(IngredientType sauceType)
	{
		hasTriggered = true;
		GameObject go = Instantiate(((sauceType == IngredientType.Ketchup) ? ketchupPrefab : mayoPrefab));
		go.transform.position = transform.position;

		// apply to something as child here for plate

		go.transform.parent = transform.parent;

		gameObject.SetActive(false);
	}
}
