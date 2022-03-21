using Photon.Pun;
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

	public bool destroyOnSauce = true;

	[SerializeField] private DishSnapPoint connectedDish = null;

	public void ProgressSauceValue(float value, IngredientType sauceType)
	{
		if (PhotonNetwork.IsMasterClient)
		{
			if (hasTriggered)
				return;
			if (sauceType == IngredientType.Ketchup)
				currentProgressTillKetchup += value;
			else
				currentProgressTillMayo += value;

			if (currentProgressTillKetchup >= timeUntillSaucePlaces)
				ApplySauce(IngredientType.Ketchup);
			else if (currentProgressTillMayo >= timeUntillSaucePlaces)
				ApplySauce(IngredientType.Mayo);
		}
	}

	private void ApplySauce(IngredientType sauceType)
	{
		GameObject go = Instantiate(((sauceType == IngredientType.Ketchup) ? ketchupPrefab : mayoPrefab));

		Debug.Log($"ketchup prog {currentProgressTillKetchup} mayo prog {currentProgressTillMayo}");

		if (connectedDish != null)
		{
			Ingredient i = go.GetComponent<Ingredient>();
			connectedDish.Snap(i);
		}
		else
		{
			go.transform.position = transform.position;
			go.transform.position += transform.up * 0.05f;
			go.transform.parent = transform.parent;
		}

		// apply to something as child here for plate

		currentProgressTillKetchup = 0f;
		currentProgressTillMayo = 0f;

		if (destroyOnSauce)
		{
			hasTriggered = true;
			gameObject.SetActive(false);
		}
	}
}
