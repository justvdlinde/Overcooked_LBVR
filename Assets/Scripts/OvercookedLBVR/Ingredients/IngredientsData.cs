using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class IngredientsData : ScriptableObject
{
    public List<IngredientData> ingredients;

    public IngredientData GetCorrespondingData(IngredientType ingredient)
    {
        return ingredients.Where(i => i.ingredientType == ingredient).First();
    }
}

[System.Serializable]
public class IngredientData
{
    public IngredientType ingredientType;
    public Ingredient ingredientPrefab;
    public Sprite ingredientIcon;
}