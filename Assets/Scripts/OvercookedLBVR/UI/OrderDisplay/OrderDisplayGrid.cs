using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Utils.Core.Attributes;
using Utils.Core.Extensions;

public class OrderDisplayGrid : MonoBehaviour
{
    [SerializeField] private float intervalSpace = 1;
    [SerializeField] private GridLayoutGroup ingredientsListGroup;
    [SerializeField] private Image ingredientIconPrefab;

    private Order currentOrder;
    private IngredientsData ingredientsData;
    private int childCount = 0;
    private List<Image> iconsInGrid = new List<Image>();

    private void Awake()
    {
        ingredientsData = Resources.Load<IngredientsData>("IngredientsData");
    }

    public void Clear()
    {
        currentOrder = null;   
        transform.RemoveAllChildren();
        iconsInGrid = new List<Image>();
    }

    public void DisplayOrder(Order order)
    {
        Clear();
        this.currentOrder = order;
        childCount = order.ingredients.Length;

        for (int i = 0; i < order.ingredients.Length; i++)
        {
            CreateIcon(ingredientsData.GetCorrespondingData(order.ingredients[i]));
        }
        ReorderChildren();
    }

    public void DisplayOrderEditor(List<IngredientType> order)
    {
        ClearEditor();
        childCount = order.Count;

        if(ingredientsData == null)
            ingredientsData = Resources.Load<IngredientsData>("IngredientsData");

        for (int i = 0; i < order.Count; i++)
        {
            CreateIcon(ingredientsData.GetCorrespondingData(order[i]));
        }
        ReorderChildren();
    }

    public void ClearEditor()
    {
        currentOrder = null;
        System.Action destroyCall = null;

        Transform[] ts = transform.GetComponentsInChildren<Transform>();
        List<Transform> tts = ts.ToList();
        if (tts.Contains(transform))
            tts.Remove(transform);
        ts = tts.ToArray();
		foreach (var item in ts)
		{
            destroyCall += () => DestroyImmediate(item.gameObject);
		}

        destroyCall?.Invoke();

        iconsInGrid = new List<Image>();
    }


    private void CreateIcon(IngredientData ingredient)
    {
        // instantiate icon prefab
        // change icon sprite
        // add to grid and list

        throw new System.NotImplementedException();
        //GameObject icon = Instantiate(ingredient.ingredientIcon, transform);
        //icons.Add(icon);
    }

    [Button]
    private void ReorderChildren()
    {
        for (int i = 0; i < childCount; i++)
        {
            Transform child = iconsInGrid[i].transform;
            child.localPosition = new Vector3(0, i * intervalSpace, 0);
        }
    }
}
