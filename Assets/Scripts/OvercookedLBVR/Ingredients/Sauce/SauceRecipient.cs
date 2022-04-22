using Photon.Pun;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SauceRecipient : MonoBehaviourPun
{
	[SerializeField] private float timeUntillSaucePlaces = 3;

	// can be merged into 1 and decide per prefab?
	[SerializeField] private GameObject ketchupPrefab = null;
	[SerializeField] private GameObject mayoPrefab = null;
	[SerializeField] private bool destroyOnSauce = true;
	[SerializeField] private FoodStackSnapPoint connectedDish = null;

	// can be merged into 1 and decide per prefab?
	private float currentProgressTillKetchup = 0;
	private float currentProgressTillMayo = 0;

	private bool hasTriggered = false;

	public void ProgressSauceValue(float value, IngredientType sauceType)
	{
        //if (PhotonNetwork.IsMasterClient)
        //{
        //    if (hasTriggered)
        //        return;
        //    if (connectedDish.CanPlaceSauce(sauceType))
        //    {
        //        if (sauceType == IngredientType.Ketchup)
        //            currentProgressTillKetchup += value;
        //        else
        //            currentProgressTillMayo += value;

        //        if (currentProgressTillKetchup >= timeUntillSaucePlaces)
        //            ApplySauce(IngredientType.Ketchup);
        //        else if (currentProgressTillMayo >= timeUntillSaucePlaces)
        //            ApplySauce(IngredientType.Mayo);
        //    }
        //}
    }

	private void ApplySauce(IngredientType sauceType)
	{
		photonView.RPC(nameof(ApplySauceRPC), RpcTarget.All, (int)sauceType);
	}

	[PunRPC]
	private void ApplySauceRPC(int ingredientTypeIndex, PhotonMessageInfo info)
    {
		IngredientType sauceType = (IngredientType)ingredientTypeIndex;
		GameObject go = Instantiate(((sauceType == IngredientType.Ketchup) ? ketchupPrefab : mayoPrefab));
 
        //if (connectedDish != null)
        //{
        //    Ingredient i = go.GetComponent<Ingredient>();
        //    connectedDish.AddToStack(i);
        //}
        //else
        //{
        //    go.transform.position = transform.position;
        //    go.transform.position += transform.up * 0.05f;
        //    go.transform.parent = transform.parent;
        //}

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
