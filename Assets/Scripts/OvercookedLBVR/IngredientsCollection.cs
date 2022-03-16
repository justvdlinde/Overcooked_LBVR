using System.Collections.Generic;
using UnityEngine;

public class IngredientsCollection : ScriptableObject
{
    public List<IngredientObjectPair> ingredients;
}

[System.Serializable]
public class IngredientObjectPair
{
    public IngredientType ingredientType;
    public GameObject ingredientPrefab;
}