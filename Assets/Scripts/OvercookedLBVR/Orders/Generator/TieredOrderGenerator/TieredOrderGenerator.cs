using System;
using UnityEngine;

public class TieredOrderGenerator : IOrderGenerator
{
	public OrderTier[] Tiers { get; private set; }
	public Action<int> TierChangedEvent;

	public int currentTier;
	public int completedOrdersInSuccession;
	public bool randomizeTimer = true;

    public TieredOrderGenerator(bool randomizeTimer, int currentTier = 0)
    {
		this.randomizeTimer = randomizeTimer;
		this.currentTier = currentTier;

		LoadTiers();
    }

	private void LoadTiers()
    {
		Tiers = Resources.Load<OrderTiersCollection>("OrderTiers").tiers;
	}

	public Order Generate()
	{
		return Tiers[currentTier].GetOrderRecipie(randomizeTimer);
	}

	public void OnOrderCompleted(bool success)
    {
		// TO DO: IMPLEMENT THIS BETTER
		//if (success)
			completedOrdersInSuccession++;
		//else
		//	completedOrdersInSuccession = 0;

		if (completedOrdersInSuccession > Tiers[currentTier].AmountToCompleteTier)
		{
			currentTier++;
			TierChangedEvent?.Invoke(currentTier);
		}
	}
}
