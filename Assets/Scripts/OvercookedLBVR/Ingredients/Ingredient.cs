using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

[SelectionBase]
public class Ingredient : MonoBehaviourPun
{
    public IngredientType IngredientType => ingredientType;
    [SerializeField] private IngredientType ingredientType = IngredientType.None;

    public IngredientStatus State => status;
    [SerializeField] private IngredientStatus status = IngredientStatus.UnProcessed;

    [Header("Cooking")]
    [SerializeField] private bool needsToBeCooked = false; 
    [SerializeField] private IngredientCookController cookComponent = null;

    // TODO: place into stackable component
	[SerializeField] private List<GameObject> toggleObjects = new List<GameObject>();

    // TODO: place into stackable component
    public bool CanStack = true;
    public Transform recentDishCollider = null;

    private void Update()
    {
        // TODO: place into stackable component
        if (!CanStack && recentDishCollider != null)
		{
            if(Vector3.Distance(recentDishCollider.transform.position, transform.position) > 0.3f)
			{
                CanStack = true;
                recentDishCollider = null;
			}
		}
    }

    // TODO: place into stackable component
	public void SetComponentsOnIngredientActive(bool active)
	{
		foreach (var item in toggleObjects)
		{
			item.SetActive(active);
		}
    }

    public bool IsPreparedProperly()
    {
        bool returnValue = State == IngredientStatus.Processed;
        if (needsToBeCooked)
            returnValue &= cookComponent.State == CookState.Cooked;
        return returnValue;
    }

    public void SetState(IngredientStatus status)
    {
        photonView.RPC(nameof(SetStateRPC), RpcTarget.All, (int)status);
    }

    [PunRPC]
    private void SetStateRPC(int statusIndex)
    {
        status = (IngredientStatus)statusIndex;
    }
}
