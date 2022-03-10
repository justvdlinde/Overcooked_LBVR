using UnityEngine;

public class Ingredient : MonoBehaviour
{
    public IngredientType ingredient = IngredientType.None;
    public IngredientStatus status = IngredientStatus.UnProcessed;
    public new Rigidbody rigidbody = null;

    public void TogglePhysics(bool toggle)
    {
        rigidbody.isKinematic = !toggle;
        rigidbody.useGravity = toggle;
    }
}
