using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class HeatSource : MonoBehaviour
{
    public List<GameObject> CookedItems => cookedItems;
    private List<GameObject> cookedItems = new List<GameObject>();

    [SerializeField] private float heatStrength = 1;

    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.TryGetComponent(out IngredientCookController cookable))
        {
            cookable.Cook(heatStrength);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.TryGetComponent(out IngredientCookController cookable))
        {
            cookable.SetCookStatus(true);
            cookedItems.Add(other.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.TryGetComponent(out IngredientCookController cookable))
        {
            if (cookable.IsCooking)
                cookable.SetCookStatus(false);
            cookedItems.Remove(other.gameObject);
        }
    }
}
