using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Utils.Core.Extensions;

public class OrderDisplayGrid : MonoBehaviour
{
    [SerializeField] private LayoutGroup grid;
    [SerializeField] private OrderDisplayIngredientIcon ingredientIconPrefab;

    private IngredientsData ingredientsData;
    private List<OrderDisplayIngredientIcon> iconsInGrid = new List<OrderDisplayIngredientIcon>();

    private void Awake()
    {
        ingredientsData = IngredientsData.Instance;
        Clear();
    }

    public void Clear()
    {
        transform.RemoveAllChildren();
        iconsInGrid.Clear();
    }

    public void DisplayOrder(Order order)
    {
        Clear();
        for (int i = 0; i < order.ingredients.Length; i++)
        {
            CreateIcon(ingredientsData.GetCorrespondingData(order.ingredients[i]));
        }
    }

    private OrderDisplayIngredientIcon CreateIcon(IngredientData ingredient)
    {
        OrderDisplayIngredientIcon icon = Instantiate(ingredientIconPrefab, grid.transform);
        icon.transform.SetAsFirstSibling();
        icon.SetSprite(ingredientsData.GetCorrespondingData(ingredient.ingredientType).ingredientIcon);
        iconsInGrid.Add(icon);
        return icon;
    }
}
