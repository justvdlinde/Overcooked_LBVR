using System.Collections.Generic;
using UnityEngine;

public class TieredOrderGenerator
{
	private OrderTier[] tiers;
	public int currentTier = 0;

    public TieredOrderGenerator()
    {
		LoadTiers();
    }

	private void LoadTiers()
    {
		tiers = Resources.Load<OrderTiersCollection>("OrderTiers").tiers;
	}

	public Order GenerateRandomOrder(int orderTier, int completedOrdersInARow, out int newOrderTier,  bool useRandomTimer = false)
	{
		currentTier = orderTier;
		if (completedOrdersInARow > tiers[currentTier].AmountToCompleteTier)
			currentTier++;

		newOrderTier = currentTier;

		return tiers[currentTier].GetOrderRecipie(useRandomTimer);
	}
}
