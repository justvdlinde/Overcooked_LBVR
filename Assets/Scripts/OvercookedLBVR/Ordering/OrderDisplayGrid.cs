using UnityEngine;
using Utils.Core.Attributes;
using Utils.Core.Extensions;

[ExecuteAlways]
public class OrderDisplayGrid : MonoBehaviour
{
    public float intervalSpace = 1;

    private Order order;
    private IngredientsData ingredientsData;

    private void Awake()
    {
        ingredientsData = Resources.Load<IngredientsData>("IngredientsData");
    }

    public void DisplayOrder(Order order)
    {
        this.order = order;
        transform.RemoveAllChildren();

        for (int i = 0; i < order.ingredients.Length; i++)
        {
            CreateIcon(ingredientsData.GetCorrespondingData(order.ingredients[i]));
        }
    }

    private void CreateIcon(IngredientData ingredient)
    {
        Instantiate(ingredient.ingredientIcon, transform);
        ReorderChildren();
    }

    [Button]
    private void ReorderChildren()
    {
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            child.localPosition = new Vector3(0, i * intervalSpace, 0);
        }
    }
}
