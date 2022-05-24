using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterSourceCollider : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        IngredientStatusCondition statusCondition = other.GetComponentInChildren<IngredientStatusCondition>();
        if (statusCondition == null)
            statusCondition = other.GetComponentInParent<IngredientStatusCondition>();
        if (statusCondition != null)
        {
            statusCondition.AddHeat(-30f, IngredientStatusCondition.StatusConditionHeatSource.Wet);
        }
    }
}
