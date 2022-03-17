using UnityEngine;

[RequireComponent(typeof(Ingredient))]
public class CookComponent : MonoBehaviour
{
    public Ingredient ingredient;
    public CookStatus status = CookStatus.Raw;

    public bool isCooking;
    public float cookAmount = 0;
    public float rawToCookTime = 1f;
    public float cookedToBurnTime = 1f;

    private void OnValidate()
    {
        if (ingredient == null)
            ingredient = GetComponent<Ingredient>();
        switch (status)
        {
            case CookStatus.Raw:
                cookAmount = 0;
                break;
            case CookStatus.Cooked:
                cookAmount = rawToCookTime;
                break;
            case CookStatus.Burned:
                cookAmount = rawToCookTime + cookedToBurnTime;
                break;
        }
    }

    public void Cook(float add = 1)
    {
        if (status != CookStatus.Burned)
        {
            if (status == CookStatus.Raw && cookAmount > rawToCookTime)
                status = CookStatus.Cooked;
            else if (status == CookStatus.Cooked && cookAmount > rawToCookTime + cookedToBurnTime)
                status = CookStatus.Burned;
        }

        cookAmount += add * Time.deltaTime;
    }
}