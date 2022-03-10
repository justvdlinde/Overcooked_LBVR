using UnityEngine;

[RequireComponent(typeof(Ingredient))]
public class CookComponent : MonoBehaviour
{
    public Ingredient ingredient;
    public CookStatus status = CookStatus.Raw;

    public bool isCooking;
    public float cookProgress = 0;
    public float rawToCookTime = 1f;
    public float cookedToBurnTime = 1f;

    private void OnValidate()
    {
        if (ingredient == null)
            ingredient = GetComponent<Ingredient>();
        switch (status)
        {
            case CookStatus.Raw:
                cookProgress = 0;
                break;
            case CookStatus.Cooked:
                cookProgress = rawToCookTime;
                break;
            case CookStatus.Burned:
                cookProgress = rawToCookTime + cookedToBurnTime;
                break;
        }
    }

    public void Cook(float add = 1)
    {
        if (status != CookStatus.Burned)
        {
            if (status == CookStatus.Raw && cookProgress > rawToCookTime)
                status = CookStatus.Cooked;
            else if (status == CookStatus.Cooked && cookProgress > rawToCookTime + cookedToBurnTime)
                status = CookStatus.Burned;
        }

        cookProgress += add * Time.deltaTime;
    }
}