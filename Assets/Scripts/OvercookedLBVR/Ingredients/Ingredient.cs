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

    public bool processToTwoAssets = false;
    [SerializeField] private GameObject result1 = null;
    [SerializeField] private GameObject result2 = null;

    public bool processToCookable = false;
    [SerializeField] private GameObject cookable = null;


    private void Awake()
	{
        unProcessedGraphics?.gameObject.SetActive(true);
        processedGraphics?.gameObject.SetActive(false);
    }

    public void Process()
	{
        // TO DO: CLEAN THIS UP
        if(!processToTwoAssets && !processToCookable)
		{
            unProcessedGraphics.gameObject.SetActive(false);
            processedGraphics.gameObject.SetActive(true);
		}
        else if(processToTwoAssets)
		{
            unProcessedGraphics.gameObject.SetActive(false);
            GameObject r1 = Instantiate(result1);
            r1.transform.position = unProcessedGraphics.transform.position;
            GameObject r2 = Instantiate(result2);
            r2.transform.position = r1.transform.position + Vector3.up * 0.05f;
        }
        else if(processToCookable)
		{
            unProcessedGraphics.gameObject.SetActive(false);
            GameObject c = Instantiate(cookable);
            c.transform.position = unProcessedGraphics.transform.position;
		}
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
