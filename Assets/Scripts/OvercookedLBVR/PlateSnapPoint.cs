using System;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class PlateSnapPoint : MonoBehaviour
{
    public bool isFilled = false;
    public Dish dish;

    private void OnValidate()
    {
        if (TryGetComponent(out Collider collider))
            collider.isTrigger = true;
    }

    public void Snap(Ingredient ingredient)
    {
        Debug.Log("snapping " + ingredient.gameObject.name + " to " + gameObject.name);

        ingredient.transform.SetParent(transform);
        ingredient.transform.localPosition = Vector3.zero;
        ingredient.transform.localEulerAngles = new Vector3(0, ingredient.transform.eulerAngles.y, 0);

        if (ingredient.TryGetComponent(out Rigidbody rigidbody))
        {
            rigidbody.velocity = Vector3.zero;
            rigidbody.isKinematic = true;
            rigidbody.useGravity = false;
        }

        isFilled = true;
        dish.ingredients.Add(ingredient);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out Ingredient ingredient))
        {
            Snap(ingredient);
        }
    }

    public bool CanBeSnappedTo()
    {
        return isFilled == false;
    }
}
