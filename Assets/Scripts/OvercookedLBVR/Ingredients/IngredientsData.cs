using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils.Core;

public class IngredientsData : ScriptableObject
{
    private static IngredientsData instance = null;

    public static IngredientsData Instance
    {
        get
        {
            if (!instance)
                instance = Resources.Load<IngredientsData>("IngredientsData");
            return instance;
        }
    }

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