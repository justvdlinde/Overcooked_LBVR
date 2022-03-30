using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Utils.Core.Attributes;

public class OrderTierManager : MonoBehaviour
{
    [SerializeField] private List<OrderTier> tiers = new List<OrderTier>();
	public int currentTier = 0;

	public List<IngredientType> order = new List<IngredientType>();

	public OrderDisplayGrid grid = null;

	[Button]
    public void GenerateRandomOrder()
	{
		order = tiers[currentTier].GetOrderRecipie();

		grid.DisplayOrderEditor(order);
	}

	private void Update()
	{
		if (Input.GetKeyDown(KeyCode.Space))
			GenerateRandomOrder();
	}
}
