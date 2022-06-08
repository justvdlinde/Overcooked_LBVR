using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class TieredRecipie : ScriptableObject
{
	[Header("RecipieOdds")]
	[Range(0, 100), SerializeField] private int topBunOdds = 100;
	[Range(0, 100), SerializeField] private int bottomBunOdds = 100;
	[Range(0, 100), SerializeField] private int firstPattyOdds = 100;
	[Range(0, 100), SerializeField] private int secondPattyOdds = 0;
	[Range(0, 100), SerializeField] private int topSauceOdds = 75;
	[Range(0, 100), SerializeField] private int bottomSauceOdds = 0;
	[SerializeField] private int topRandomIngredientsAmount = 0;
	[SerializeField] private int bottomRandomIngredientsAmount = 0;

	[Header("Timer values")]
	[Range(0, 100), SerializeField] private int timerDeviationPercentage = 5;
	[SerializeField] private float timeForRecipieInSeconds = 60;

	public int TopBunOdds { get => topBunOdds; }
	public int BottomBunOdds { get => bottomBunOdds; }
	public int FirstPattyOdds { get => firstPattyOdds; }
	public int SecondPattyOdds { get => secondPattyOdds; }
	public int TopSauceOdds { get => topSauceOdds; }
	public int BottomSauceOdds { get => bottomSauceOdds; }
	public int TopRandomIngredientsAmount { get => topRandomIngredientsAmount; }
	public int BottomRandomIngredientsAmount { get => bottomRandomIngredientsAmount; }

	public float GetTimeForRecipie(bool useRandomOffset = false)
	{
		float offset = Mathf.Lerp(-timerDeviationPercentage, timerDeviationPercentage, (float)(GenerateRandom0to100() * 0.01f));

		//offset *= timeForRecipieInSeconds;

		return timeForRecipieInSeconds + offset;
	}

	public List<IngredientType> GetRecipie()
	{
		List<IngredientType> returnList = new List<IngredientType>();

		bool addSecondPatty = false;
		if (GenerateRandom0to100() < secondPattyOdds && (bottomRandomIngredientsAmount > 0 || topRandomIngredientsAmount > 0))
			addSecondPatty = true;

		bool addSecondPattyToBottom = false;
		if (GenerateRandom0to100() < 50 && bottomRandomIngredientsAmount > 0)
			addSecondPattyToBottom = true;

		if (GenerateRandom0to100() < bottomBunOdds)
			returnList.Add(IngredientType.BunBottom);

		if (GenerateRandom0to100() < bottomSauceOdds)
			returnList.Add(GenerateRandomSauce());

		List<IngredientType> bottomIngredients = new List<IngredientType>();
		for (int i = 0; i < bottomRandomIngredientsAmount; i++)
		{
			bottomIngredients.Add(GenerateRandomIngredient());
		}

		if(addSecondPatty && addSecondPattyToBottom)
		{
			int rdm = Random.Range(0, bottomIngredients.Count);
			bottomIngredients[rdm] = IngredientType.Patty;
		}

		returnList.AddRange(bottomIngredients);

		if (GenerateRandom0to100() < firstPattyOdds)
			returnList.Add(IngredientType.Patty);

		if (GenerateRandom0to100() < topSauceOdds)
			returnList.Add(GenerateRandomSauce());

		List<IngredientType> topIngredients = new List<IngredientType>();
		for (int i = 0; i < topRandomIngredientsAmount; i++)
		{
			topIngredients.Add(GenerateRandomIngredient());
		}

		if (addSecondPatty && !addSecondPattyToBottom)
		{
			int rdm = Random.Range(0, topIngredients.Count);
			topIngredients[rdm] = IngredientType.Patty;
		}
		returnList.AddRange(topIngredients);


		if (GenerateRandom0to100() < topBunOdds)
			returnList.Add(IngredientType.BunTop);

		return returnList;
	}

	private IngredientType GenerateRandomSauce()
	{
		int isKetchup = Random.Range(-99, 100);
		return (isKetchup < 0) ? IngredientType.Ketchup : IngredientType.Mayo;
	}

	private IngredientType GenerateRandomIngredient()
	{
		List<IngredientType> availableIngredients = new List<IngredientType>{ 
		IngredientType.Lettuce,
		IngredientType.Tomato,
		IngredientType.Cheese,
		IngredientType.Onion,
		IngredientType.Bacon
		};

		int number = Random.Range(0, availableIngredients.Count);
		IngredientType returnType = availableIngredients[Random.Range(0, availableIngredients.Count)];
		return returnType;
	}

	private int GenerateRandom0to100()
	{
		return (int)Random.Range(0, 100);
	}
}
