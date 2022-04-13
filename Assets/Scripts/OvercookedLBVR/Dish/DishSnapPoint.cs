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
        if (other.TryGetComponent(out Ingredient item))
        {
            if (item.GetComponent<Rigidbody>().TryGetComponent(out PhotonView photonView))
            {
                if (photonView.IsMine)
                {
                    if (!dish.ingredients.Contains(item) && item.Status == IngredientStatus.Processed)
                        Snap(item);
                }
            }
        }
    }

    public void Snap(Ingredient ingredient)
    {
        if (!ingredient.CanStack)
		{
            Debug.Log("Ingredient cant stack");
            return;
		}

        dish.AddIngredient(ingredient);
    }

    public Vector3 GetTopSnapPosition(GameObject obj)
    {
		return new Vector3(0, totalStackHeight + GetObjectHeight(obj), 0);
    }

    private float GetObjectHeight(GameObject obj)
	{
        float objectHeight = 0;
        if (obj.transform.GetChild(0).TryGetComponent(out MeshFilter renderer))
        {
            objectHeight = renderer.mesh.bounds.size.y * renderer.transform.localScale.y;
        }

        if (objectHeight < 0.015f)
            objectHeight = 0.015f;

        return objectHeight;
    }

    public DummyToolHandle RemoveTopIngredient()
    {
        if (dish.ingredients.Count <= 0)
            return null;

        Ingredient ingredient = dish.ingredients[dish.ingredients.Count - 1];
        if (ingredient.IngredientType == IngredientType.Ketchup || ingredient.IngredientType == IngredientType.Mayo)
		{
            return null;
        }

        return dish.RemoveTopIngredient(ingredient);
    }

    public void RecomputeStackHeight()
	{
        stackElements = new List<float>();
        totalStackHeight = 0.0f;
		foreach (var item in dish.ingredients)
		{
            Vector3 stackHeight = GetTopSnapPosition(item.gameObject); //item.processedGraphics.gameObject
            stackElements.Add(stackHeight.y);
            totalStackHeight = stackHeight.y;
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
        if(dish.ingredients.Count > 0)
		{
            return dish.ingredients[dish.ingredients.Count - 1].IngredientType != sauce;
		}
        else
            return false;
	}
}
