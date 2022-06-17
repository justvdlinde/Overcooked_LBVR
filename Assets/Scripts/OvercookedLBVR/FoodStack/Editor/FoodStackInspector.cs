using Photon.Pun;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(FoodStack))]
public class FoodStackInspector : Editor
{
    private FoodStack foodStack;
    private IngredientType ingredientType = IngredientType.BunBottom;
    private IngredientsData ingredientsData;
    private SerializedProperty ingredientsStack;
    private bool destroyOnRemove = true;

    private void OnEnable()
    {
        foodStack = target as FoodStack;
        ingredientsData = IngredientsData.Instance;
        ingredientsStack = serializedObject.FindProperty("ingredientsStack");
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        GUI.enabled = Application.isPlaying;
        EditorGUILayout.BeginVertical("box");
        DrawIngredientsList();
        GUI.enabled = Application.isPlaying;

        EditorGUILayout.Space(10);
        DrawHelperButtons();

        serializedObject.ApplyModifiedProperties();
        EditorGUILayout.EndVertical();
    }

    private void DrawIngredientsList()
    {
        EditorGUILayout.LabelField("Ingredients", EditorStyles.boldLabel);
        serializedObject.Update();

        //ingredientsStack.arraySize = EditorGUILayout.IntField("Size", ingredientsStack.arraySize);
        EditorGUI.indentLevel++;
        float size = Screen.width / 4;
        if (ingredientsStack.arraySize == 0)
        {
            EditorGUILayout.LabelField("Empty");
        }
        else
        {
            for (int i = 0; i < ingredientsStack.arraySize; i++)
            {
                SerializedProperty ingredientProperty = ingredientsStack.GetArrayElementAtIndex(i);
                Ingredient ingredient = foodStack.IngredientsStack[i];
                EditorGUILayout.BeginHorizontal();
                //EditorGUILayout.PropertyField(inredientProperty, new GUIContent(i + " " + ingredient.name), true);
                EditorGUILayout.LabelField(i + ": " + ingredient.IngredientType, GUILayout.Width(size));

                //EditorGUILayout.LabelField(ingredient.State.ToString(), GUILayout.Width(size));
                EditorGUI.BeginChangeCheck();
                IngredientStatus state = (IngredientStatus)EditorGUILayout.EnumPopup(ingredient.State, GUILayout.Width(size));
                if (state != ingredient.State)
                    ingredient.SetState(state);

                if (ingredient.NeedsToBeCooked)
                {
                    CookState cookState = (CookState)EditorGUILayout.EnumPopup(ingredient.CookController.State, GUILayout.Width(size));
                    if (cookState != ingredient.CookController.State)
                        ingredient.CookController.SetState(cookState);
                }
                EditorGUILayout.EndHorizontal();
            }
        }
        EditorGUI.indentLevel--;
    }

    private void DrawHelperButtons()
    {
        bool guiWasEnabled = GUI.enabled;

        EditorGUILayout.BeginHorizontal();
        ingredientType = (IngredientType)EditorGUILayout.EnumPopup(ingredientType);
        GUI.enabled = guiWasEnabled && ingredientType != IngredientType.None;
        if (GUILayout.Button("Add to stack"))
            AddIngredientToStack(ingredientType);
        EditorGUILayout.EndHorizontal();

        GUI.enabled = guiWasEnabled;
        EditorGUILayout.BeginHorizontal();
        if (GUILayout.Button("Add minimum burger requirements"))
        {
            AddIngredientToStack(IngredientType.BunBottom);
            AddIngredientToStack(IngredientType.Patty);
            AddIngredientToStack(IngredientType.BunTop);
        }
        EditorGUILayout.EndHorizontal();

        EditorGUILayout.BeginHorizontal();
        destroyOnRemove = EditorGUILayout.Toggle("Destroy", destroyOnRemove);
        GUI.enabled = guiWasEnabled && foodStack.IngredientsStack.Count > 0;
        if (GUILayout.Button("Remove top"))
        {
            foodStack.RemoveTopIngredient(destroyOnRemove);
        }
        if (GUILayout.Button("Remove all"))
        {
            foodStack.RemoveAllIngredients(destroyOnRemove);
        }
        EditorGUILayout.EndHorizontal();

        GUI.enabled = true;
    }

    private void AddIngredientToStack(IngredientType ingredientType)
    {
        if (ingredientType == IngredientType.None)
            return;

        Ingredient prefab = ingredientsData.GetCorrespondingData(ingredientType).ingredientPrefab;
        Ingredient ingredient = PhotonNetwork.Instantiate(prefab.name, foodStack.transform.position, foodStack.transform.rotation).GetComponent<Ingredient>();
        if (ingredient.ChopController != null)
            ingredient.ChopController.ProcessIngredient();
        foodStack.AddIngredientToStack(ingredient);
    }
}
