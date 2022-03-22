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

    private void OnValidate()
    {
        if(collider != null)
            collider.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Ingredient item))
        {
			Debug.Log("Got item");
            if(!dish.ingredients.Contains(item) && item.status == IngredientStatus.Processed)
                Snap(item);
        }
    }

    public void Snap(Ingredient ingredient)
    {
        Rigidbody ingredientParent = ingredient.rigidbody;

        ingredientParent.transform.SetParent(transform);
        Vector3 snapPosition = GetTopSnapPosition(ingredient.processedGraphics.gameObject);
        stackElements.Add(snapPosition.y);
        totalStackHeight = snapPosition.y;
        ingredientParent.transform.localPosition = snapPosition;
        ingredientParent.transform.localEulerAngles = new Vector3(0, ingredientParent.transform.eulerAngles.y, 0);
        UpdateTriggerSize();

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
        float objectHeight = 0;
        if(obj.transform.GetChild(0).TryGetComponent(out MeshFilter renderer))
        {
            objectHeight = renderer.mesh.bounds.size.y * renderer.transform.localScale.y;
        }
        return new Vector3(0, totalStackHeight + objectHeight * snapMargin, 0);
    }

    public void RemoveTopIngredient()
    {
        Ingredient ingredient = dish.ingredients[dish.ingredients.Count - 1];
        Rigidbody ingredientParent = ingredient.rigidbody;
        ingredientParent.isKinematic = false;
        ingredientParent.useGravity = true;
        ingredientParent.GetComponent<Tool>().enabled = false;

		ingredient.SetComponentsOnIngredientActive(true);


		Collider[] colliders = ingredientParent.GetComponentsInChildren<Collider>(true);
        foreach (Collider collider in colliders)
            collider.enabled = true;

        float ingredientHeight = stackElements.Count - 1;
        stackElements.RemoveAt(stackElements.Count - 1);
        totalStackHeight -= ingredientHeight;

        UpdateTriggerSize();
    }

    private void UpdateTriggerSize()
    {
        Vector3 colliderPosition = collider.center;
        colliderPosition.y = totalStackHeight + collider.size.y * 0.5f;
        collider.center = colliderPosition;
    }
}
