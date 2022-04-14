using Photon.Pun;
using UnityEngine;

[SelectionBase]
public class Ingredient : MonoBehaviourPun
{
    public IngredientType IngredientType => ingredientType;
    public IngredientStatus State => status;
    public IngredientSnapController SnapController => snapController;

    [SerializeField] private IngredientType ingredientType = IngredientType.None;
    [SerializeField] private IngredientStatus status = IngredientStatus.UnProcessed;

    [SerializeField] private bool needsToBeCooked = false; 

    [Header("References")]
    [Tooltip("Optional, only needed if 'needsToBeCooked' is set to true")]
    [SerializeField] private IngredientCookController cookController = null;
    [Tooltip("Optional, only needed if object is snappable to dish and contains a IngredientSnapController component")]
    [SerializeField] private IngredientSnapController snapController = null;

    public bool IsPreparedProperly()
    {
        bool returnValue = State == IngredientStatus.Processed;
        if (needsToBeCooked)
            returnValue &= cookController.State == CookState.Cooked;
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
