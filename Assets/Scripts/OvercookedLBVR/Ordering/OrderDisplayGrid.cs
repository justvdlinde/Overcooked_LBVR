using System.Collections.Generic;
using UnityEngine;
using Utils.Core.Attributes;
using Utils.Core.Extensions;

public class OrderDisplayGrid : MonoBehaviour
{
    public float intervalSpace = 1;

    private Order order;
    private IngredientsData ingredientsData;
    private int childCount = 0;
    private List<GameObject> icons = new List<GameObject>();

    private void Awake()
    {
        ingredientsData = Resources.Load<IngredientsData>("IngredientsData");
    }

    public void Clear()
    {
        order = null;   
        transform.RemoveAllChildren();
        icons = new List<GameObject>();
    }

    public void DisplayOrder(Order order)
    {
        Clear();
        this.order = order;
        childCount = order.ingredients.Length;

        for (int i = 0; i < order.ingredients.Length; i++)
        {
            CreateIcon(ingredientsData.GetCorrespondingData(order.ingredients[i]));
        }
        ReorderChildren();
    }

    private void CreateIcon(IngredientData ingredient)
    {
        GameObject icon = Instantiate(ingredient.ingredientIcon, transform);
        icons.Add(icon);
    }

    [Button]
    private void ReorderChildren()
    {
        for (int i = 0; i < childCount; i++)
        {
            Transform child = icons[i].transform;
            child.localPosition = new Vector3(0, i * intervalSpace, 0);
        }
    }
}
