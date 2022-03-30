using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils.Core.Attributes;

public class OrderTierManager : MonoBehaviour
{
    [SerializeField] private List<OrderTier> tiers = new List<OrderTier>();
	public int currentTier = 0;

	public Order GenerateRandomOrder(int orderTier, int completedOrdersInARow, out int newOrderTier,  bool useRandomTimer = false)
	{
		currentTier = orderTier;
		if (completedOrdersInARow > tiers[currentTier].AmountToCompleteTier)
			currentTier++;

		newOrderTier = currentTier;

		return tiers[currentTier].GetOrderRecipie(useRandomTimer);
	}
}
