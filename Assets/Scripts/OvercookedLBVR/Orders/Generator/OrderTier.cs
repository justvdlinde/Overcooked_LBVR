using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class OrderTier : ScriptableObject
{
    [SerializeField] private int tier = 0;
    [SerializeField] private int amountToCompleteTier = 0;

	public int Tier { get => tier; }
	public int AmountToCompleteTier { get => amountToCompleteTier; }

	[SerializeField] private Dictionary<TieredRecipie, int> TierContent = new Dictionary<TieredRecipie, int>();

	[SerializeField] private List<TieredBurgerOdds> tieredRecipieOdds = new List<TieredBurgerOdds>();

	private void Awake()
	{
		SerializeListToDict();
	}

	private void OnValidate()
	{
		SerializeListToDict();
	}

	public Order GetOrderRecipie(bool useRandom = false)
	{
		List<IngredientType> returnList = new List<IngredientType>();

		int odds = 0;
		foreach (var item in tieredRecipieOdds)
		{
			odds += item.odds;
		}

		int rdm = Random.Range(0, odds);
		TieredRecipie recipie = null;

		foreach (var item in tieredRecipieOdds)
		{
			rdm -= item.odds;
			if (rdm <= 0)
			{
				recipie = item.tieredRecipie;
				break;
			}
		}

		IngredientType[] ingredients = recipie.GetRecipie().ToArray();
		NetworkedTimer timer = new NetworkedTimer();
		timer.Set(recipie.GetTimeForRecipie(useRandom));
		Order order = new Order(ingredients, timer);

		//returnList.AddRange(recipie.GetRecipie(useRandom));

		return order;
	}

	private void SerializeListToDict()
	{
		if (tieredRecipieOdds == null)
			return;
		TierContent = new Dictionary<TieredRecipie, int>();
		foreach (var item in tieredRecipieOdds)
		{
			if(!TierContent.ContainsKey(item.tieredRecipie))
				TierContent.Add(item.tieredRecipie, item.odds);
		}
	}
}

[System.Serializable]
public struct TieredBurgerOdds
{
	public int odds;
	public TieredRecipie tieredRecipie;
}