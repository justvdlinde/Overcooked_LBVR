using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FoodStack))]
public class FoodStackInspector : Editor
{
    private FoodStack foodStack;
    private IngredientType ingredientType;
    private IngredientsData ingredientsData;

    private void OnEnable()
    {
        foodStack = target as FoodStack;
        ingredientsData = Resources.Load<IngredientsData>("IngredientsData");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUI.enabled = Application.isPlaying;
        EditorGUILayout.LabelField("Debugging", EditorStyles.boldLabel);
        ingredientType = (IngredientType)EditorGUILayout.EnumPopup("Ingredient", ingredientType);
        if (GUILayout.Button("Add to stack"))
            OnAddToStackButtonSelected();
        if (GUILayout.Button("Add to stack"))
            foodStack.RemoveTopIngredient();
        GUI.enabled = true;
    }

    private void OnAddToStackButtonSelected()
    {
        if (ingredientType == IngredientType.None)
            return;

        GameObject prefab = ingredientsData.GetCorrespondingData(ingredientType).ingredientPrefab;
        Ingredient ingredient = PhotonNetwork.Instantiate(prefab.name, foodStack.transform.position, foodStack.transform.rotation).GetComponent<Ingredient>();
        foodStack.AddIngredientToStack(ingredient);
    }
}
