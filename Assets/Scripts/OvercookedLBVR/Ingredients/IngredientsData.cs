using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// TODO: change to singleton and look itself up using const path
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