using Photon.Pun;
using System.Collections.Generic;
using UnityEngine;

[SelectionBase]
public class Ingredient : MonoBehaviourPun
{
    public IngredientType IngredientType => ingredientType;
    public IngredientStatus State => status;
    public IngredientSnapController SnapController => snapController;
    public PhysicsCharacter.Tool GrabController => grabController;
    public bool NeedsToBeCooked => needsToBeCooked;
    public IngredientCookController CookController => cookController;
    public IngredientChopController ChopController => chopController;

    [SerializeField] private IngredientType ingredientType = IngredientType.None;
    [SerializeField] private IngredientStatus status = IngredientStatus.UnProcessed;
    [SerializeField] private bool needsToBeCooked = false; 

    [Header("References")]
    [Tooltip("Optional, only needed if 'needsToBeCooked' is set to true")]
    [SerializeField] private IngredientCookController cookController = null;
    [Tooltip("Optional, only needed if object is snappable to dish and contains a IngredientSnapController component")]
    [SerializeField] private IngredientSnapController snapController = null;
    [Tooltip("Optional, only needed if object is choppable")]
    [SerializeField] private IngredientChopController chopController = null;
    [Tooltip("Optional, only needed if object is grabbable")]
    [SerializeField] private PhysicsCharacter.Tool grabController = null;
    [Tooltip("Optional, only needed if object is status conditionable")]
    [SerializeField] private IngredientStatusCondition statusConditionManager = null;

    [SerializeField] private List<Collider> colliders = null;

    public bool CanIngredientCook()
	{
        if (statusConditionManager == null)
            return true;
        else
            return statusConditionManager.CanCook;
	}

    public bool IsPreparedProperly()
    {
        bool returnValue = State == IngredientStatus.Processed;
        if (needsToBeCooked)
            returnValue &= cookController.State == CookState.Cooked;
        return returnValue;
    }


	public void SetCollisionsIgnored(Collider col, bool isIgnored)
	{
		if (colliders == null)
			return;
		foreach (var item in colliders)
		{
			Physics.IgnoreCollision(col, item, isIgnored);
		}
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

    public bool CanBeGrabbed()
    {
        return grabController != null;
    }

    public override string ToString()
    {
        return ingredientType.ToString() + "(" + status + (CookController != null ? "-" + CookController.State : string.Empty) + ")";
    }

    public CookState GetCookState()
	{
        if (!needsToBeCooked)
            return CookState.Raw;

        if (cookController != null)
            return cookController.State;
        
        // default
        return CookState.Raw;
	}
}
