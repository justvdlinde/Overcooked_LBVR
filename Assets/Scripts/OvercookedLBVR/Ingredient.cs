using UnityEngine;

public class Ingredient : MonoBehaviour
{
    public IngredientType ingredientType = IngredientType.None;
    public IngredientStatus status = IngredientStatus.UnProcessed;
    public new Rigidbody rigidbody = null;

    public bool needsToBeCooked = false;
    public CookComponent cookComponent = null;

    public void TogglePhysics(bool toggle)
    {
        rigidbody.isKinematic = !toggle;
        rigidbody.useGravity = toggle;
    }

    public bool IsCookedProperly()
    {
        bool returnValue = status == IngredientStatus.Processed;
        if (needsToBeCooked)
            returnValue &= cookComponent.status == CookStatus.Cooked;
        return returnValue;
    }
}
