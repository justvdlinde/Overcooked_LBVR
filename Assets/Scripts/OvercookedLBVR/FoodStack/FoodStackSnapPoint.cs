using UnityEngine;

[RequireComponent(typeof(Collider))]
public class FoodStackSnapPoint : MonoBehaviour
{
    public FoodStack foodStack;

    public float colliderPadding = 0.1f;
    public float snapMargin = 0.8f;

  

    private void OnValidate()
    {
        if (TryGetComponent(out Collider collider))
            collider.isTrigger = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.TryGetComponent(out IngredientSnapController snappable))
        {
            if (foodStack.CanAddToStack(snappable))
                foodStack.AddIngredientToStack(snappable.Ingredient);
        }
    }

    public void SetHeight(float height)
	{
        Vector3 newPos = transform.localPosition;
        newPos.y = height;
        transform.localPosition = newPos;
	}
}
