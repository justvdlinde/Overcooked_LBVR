using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TooltipObject : MonoBehaviour
{
    public static List<TooltipObject> tooltipObjects = new List<TooltipObject>();


    private void Awake()
    {
        tooltipObjects.Add(this);
    }

    private void OnDestroy()
    {
        tooltipObjects.Remove(this);
    }
}
