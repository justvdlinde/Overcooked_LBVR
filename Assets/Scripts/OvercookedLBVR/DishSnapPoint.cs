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

    [SerializeField] private Transform ingredientStack = null;

    private void OnValidate()
    {
        if(collider != null)
            collider.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Ingredient item))
        {
            if(!dish.ingredients.Contains(item) && item.Status == IngredientStatus.Processed)
                Snap(item);
        }
    }

    public void Snap(Ingredient ingredient)
    {
        if (!ingredient.CanStack)
		{
            Debug.Log("Ingredient cant stack");
            return;
		}
        Rigidbody ingredientParent = ingredient.rigidbody;
        if(ingredientParent.TryGetComponent(out PhotonRigidbodyView view))
        {
            view.enabled = false;
        }

        ingredientParent.transform.SetParent(ingredientStack);
        Vector3 snapPosition = GetTopSnapPosition(ingredient.processedGraphics.gameObject);
        stackElements.Add(snapPosition.y);
        float diff = snapPosition.y - totalStackHeight;
        totalStackHeight = snapPosition.y;
        Vector3 pos = snapPosition;
        snapPosition.y -= diff * 0.5f;
        ingredientParent.transform.localPosition = snapPosition;
        ingredientParent.transform.localEulerAngles = new Vector3(0, ingredientParent.transform.eulerAngles.y, 0);
        UpdateTriggerPosition();
        //UpdateTriggerSize();

        Collider[] colliders = ingredientParent.GetComponentsInChildren<Collider>(true);
        foreach (Collider collider in colliders)
        {
            if(!collider.isTrigger)
                collider.enabled = false;
        }

        ingredientParent.velocity = Vector3.zero;
        ingredientParent.isKinematic = true;
        ingredientParent.useGravity = false;
		Tool t = ingredientParent.GetComponent<Tool>();
		if(t != null)
			ingredientParent.GetComponent<Tool>().enabled = false;

        ingredient.SetComponentsOnIngredientActive(false);

        dish.ingredients.Add(ingredient);
    }

    private Vector3 GetTopSnapPosition(GameObject obj)
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

        Ingredient top = dish.ingredients[dish.ingredients.Count - 1];
        if (top.ingredientType == IngredientType.Ketchup || top.ingredientType == IngredientType.Mayo)
		{
            return null;
        }

        Ingredient ingredient = dish.ingredients[dish.ingredients.Count - 1];
        Rigidbody ingredientParent = ingredient.rigidbody;
        ingredientParent.isKinematic = false;
        ingredientParent.useGravity = true;
        ingredientParent.transform.parent = null;
        Tool t = ingredientParent.GetComponent<Tool>();
        if(t != null)
            t.enabled = true;

        DummyToolHandle handle = ingredientParent.GetComponentInChildren<DummyToolHandle>(true);

		ingredient.SetComponentsOnIngredientActive(true);

		Collider[] colliders = ingredientParent.GetComponentsInChildren<Collider>(true);
        foreach (Collider collider in colliders)
            collider.enabled = true;


        if (dish.ingredients.Contains(ingredient))
            dish.ingredients.Remove(ingredient);

        ingredient.CanStack = false;
        ingredient.recentDishCollider = transform;

        RecomputeStackHeight();
        UpdateTriggerPosition();
        //UpdateTriggerSize();
        return handle;
    }

    private void RecomputeStackHeight()
	{
        stackElements = new List<float>();
        totalStackHeight = 0.0f;
		foreach (var item in dish.ingredients)
		{
            Vector3 stackHeight = GetTopSnapPosition(item.processedGraphics.gameObject);
            stackElements.Add(stackHeight.y);
            totalStackHeight = stackHeight.y;
		}
	}

    private void UpdateTriggerPosition()
	{
        Vector3 pos = transform.localPosition;
		pos.y = totalStackHeight;
        transform.localPosition = pos;
	}

    private void UpdateTriggerSize()
    {
        Vector3 colliderPosition = collider.center;
        colliderPosition.y = totalStackHeight + collider.size.y * 0.5f;
        collider.center = colliderPosition;
    }

    public bool CanPlaceSauce(IngredientType sauce)
	{
        if(dish.ingredients.Count > 0)
		{
            return dish.ingredients[dish.ingredients.Count - 1].ingredientType != sauce;
		}
        else
            return false;
	}
}
