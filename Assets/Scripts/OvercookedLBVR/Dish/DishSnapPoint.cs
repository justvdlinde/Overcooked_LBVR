using Photon.Pun;
using PhysicsCharacter;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DishSnapPoint : MonoBehaviour
{
    public Dish dish;
    public new BoxCollider collider;
    public float colliderPadding = 0.1f;
    public float snapMargin = 0.8f;

    public List<float> stackElements;
    public float totalStackHeight;

    public Transform ingredientStack = null;

    private void OnValidate()
    {
        if(collider != null)
            collider.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("OnTriggerEnter " + other);
        if (other.TryGetComponent(out IngredientSnapController snappable))
        {
            Ingredient ingredient = snappable.Ingredient;
            if (ingredient.photonView.IsMine)
            {
                if (!dish.ingredientsStack.Contains(ingredient) && snappable.CanBeSnapped())
                    AddToStack(snappable);
            }
        }
    }

    public void AddToStack(IngredientSnapController snappable)
    {
        Debug.Log("addToStack " + snappable.Ingredient);
        dish.AddIngredientToStack(snappable.Ingredient);
    }

    public Vector3 GetTopSnapPosition(IngredientSnapController snappable)
    {
		return new Vector3(0, totalStackHeight + snappable.GetGraphicHeight(), 0);
    }

    public DummyToolHandle RemoveTopIngredient()
    {
        if (dish.ingredientsStack.Count <= 0)
            return null;

        Ingredient ingredient = dish.ingredientsStack[dish.ingredientsStack.Count - 1];

        // TODO: check if ingredient has grab component instead?
        if (ingredient.IngredientType == IngredientType.Ketchup || ingredient.IngredientType == IngredientType.Mayo)
            return null;

        return dish.RemoveTopIngredient(ingredient);
    }

    public void RecomputeStackHeight()
	{
        stackElements = new List<float>();
        totalStackHeight = 0.0f;
		foreach (var ingredient in dish.ingredientsStack)
		{
            //Vector3 stackHeight = GetTopSnapPosition(ingredient); //item.processedGraphics.gameObject
            //stackElements.Add(stackHeight.y);
            //totalStackHeight = stackHeight.y;
		}
	}

    public void UpdateTriggerPosition()
	{
        Vector3 pos = transform.localPosition;
		pos.y = totalStackHeight;
        transform.localPosition = pos;
	}

    public void UpdateTriggerSize()
    {
        Vector3 colliderPosition = collider.center;
        colliderPosition.y = totalStackHeight + collider.size.y * 0.5f;
        collider.center = colliderPosition;
    }

    public bool CanPlaceSauce(IngredientType sauce)
	{
        if(dish.ingredientsStack.Count > 0)
		{
            return dish.ingredientsStack[dish.ingredientsStack.Count - 1].IngredientType != sauce;
		}
        else
            return false;
	}
}
