using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CookComponentVisualController : MonoBehaviour
{
    [SerializeField] private CookComponent cookComponent = null;
    [SerializeField] private MeshRenderer mesh = null;
    [SerializeField] private Color cookedColor = Color.grey;
    [SerializeField] private Color burnedColor = Color.black;

    private Material material;
    private Color normalColor;

    private void Start()
    {
        material = mesh.material;
        normalColor = material.color;
    }

    private void Update()
    {
        if (cookComponent == null)
            return;

        if (cookComponent.status == CookStatus.Raw)
        {
            material.color = Color.Lerp(normalColor, cookedColor, cookComponent.cookAmount / cookComponent.rawToCookTime);
        }
        else if(cookComponent.status == CookStatus.Cooked)
        {
            material.color = Color.Lerp(normalColor, burnedColor, cookComponent.cookAmount / (cookComponent.rawToCookTime + cookComponent.cookedToBurnTime));
        }
    }
}
