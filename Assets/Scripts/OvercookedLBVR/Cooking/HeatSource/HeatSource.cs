using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class HeatSource : MonoBehaviour
{
    public List<Ingredient> CookedItems => cookedItems;
    private List<Ingredient> cookedItems = new List<Ingredient>();

    [SerializeField] private float heatStrength = 1;

    [SerializeField] private GrillCover grillCover = null;

    private void OnTriggerStay(Collider other)
    {
        float heatMultiplier = 1.0f;
        if (grillCover != null)
            heatMultiplier *= ((grillCover.IsHoodClosed) ? 1.5f : 1.0f);

        if (other.gameObject.TryGetComponent(out IngredientCookController cookable))
        {
            if (grillCover != null)
			{
                if (cookable.IsCookable)
                   cookable.Cook(heatStrength * heatMultiplier, grillCover.IsHoodClosed);
			}
            else
                cookable.Cook(heatStrength * heatMultiplier, false);
        }
        IngredientStatusCondition statusCondition = other.GetComponentInChildren<IngredientStatusCondition>();
        if(statusCondition == null)
            statusCondition = other.GetComponentInParent<IngredientStatusCondition>();
        if (statusCondition != null)
		{
            statusCondition.AddHeat(Time.deltaTime * 10f, StatusConditionHeatSource.Heat);
		}
    }

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("OntriggerEnter " + other);
        if (other.gameObject.TryGetComponent(out IngredientCookController cookable))
        {
            if (cookable.IsCookable)
            {
                cookable.SetCookStatus(true);
                cookedItems.Add(cookable.Ingredient);
                cookable.Ingredient.DestroyEvent += OnIngredientDestroyEvent;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("OntriggerExit " + other);
        if (other.gameObject.TryGetComponent(out IngredientCookController cookable))
        {
            if (cookable.IsCooking)
                cookable.SetCookStatus(false);
            cookedItems.Remove(cookable.Ingredient);
            cookable.Ingredient.DestroyEvent -= OnIngredientDestroyEvent;
        }
    }

    private void OnIngredientDestroyEvent(Ingredient ingredient)
    {
        cookedItems.Remove(ingredient);
    }
}
