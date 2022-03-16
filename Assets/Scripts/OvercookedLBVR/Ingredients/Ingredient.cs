using UnityEngine;

public class Ingredient : MonoBehaviour
{
    public IngredientType ingredientType = IngredientType.None;
    public IngredientStatus status = IngredientStatus.UnProcessed;
    public new Rigidbody rigidbody = null;

    public bool needsToBeCooked = false;
    public CookComponent cookComponent = null;

    [SerializeField] private Transform unProcessedGraphics = null;
    [SerializeField] private Transform processedGraphics = null;

	private void Awake()
	{
        unProcessedGraphics.gameObject.SetActive(true);
        processedGraphics.gameObject.SetActive(false);
    }

    public void Process()
	{
        unProcessedGraphics.gameObject.SetActive(false);
        processedGraphics.gameObject.SetActive(true);
        status = IngredientStatus.Processed;
	}

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
