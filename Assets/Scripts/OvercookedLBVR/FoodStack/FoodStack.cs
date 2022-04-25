using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum DishResult
{
    None,
    Delivered,
    TimerExceeded
}

public class FoodStack : MonoBehaviourPunCallbacks
{
    public Action<Ingredient> IngredientAddedEvent;
    public List<Ingredient> IngredientsStack => ingredientsStack;

    [SerializeField] private FoodStackSnapPoint snapPoint = null;
    [SerializeField] private Transform stackContainer = null;
    [SerializeField] private bool firstIngredientMustBeBunBottom = true;
    [SerializeField] private bool lastIngredientMustBeBunTop = true;

    private List<Ingredient> ingredientsStack = new List<Ingredient>();
    private List<float> ingredientHeights = new List<float>();
    private float totalStackHeight;

    public override void OnPlayerEnteredRoom(PhotonNetworkedPlayer newPlayer)
    {
        if (PhotonNetwork.IsMasterClient)
            SendSyncData(newPlayer);
    }

    private void SendSyncData(PhotonNetworkedPlayer player)
    {
        int[] ingredientIds = new int[ingredientsStack.Count];
        for (int i = 0; i < ingredientsStack.Count; i++)
        {
            ingredientIds[i] = ingredientsStack[i].photonView.ViewID;
        }
        photonView.RPC(nameof(SendSyncDataRPC), player, ingredientIds);
    }

    [PunRPC]
    private void SendSyncDataRPC(object data)
    {
        int[] ids = (int[])data;
        for(int i = 0; i < ids.Length; i++)
        {
            Ingredient ingredient = PhotonView.Find(ids[i]).GetComponent<Ingredient>();
            AddIngredientToStackInternal(ingredient);
        }
    }

    public bool CanAddToStack(IngredientSnapController ingredientSnapper)
    {
        if (!ingredientSnapper.Ingredient.photonView.IsMine)
            return false;

        if (firstIngredientMustBeBunBottom)
        {
            if (ingredientsStack.Count > 0 && ingredientsStack[ingredientsStack.Count - 1].IngredientType == IngredientType.BunTop)
                return false;
        }

        if (!ingredientsStack.Contains(ingredientSnapper.Ingredient) && ingredientSnapper.CanBeSnapped())
        {
            // If stack is empty, only stack if ingredient is bottom bun
            if (lastIngredientMustBeBunTop)
            {
                if (IngredientsStack.Count == 0)
                    return ingredientSnapper.Ingredient.IngredientType == IngredientType.BunBottom;
                else
                    return true;
            }
            else
            {
                return true;
            }
        }

        return false;
    }

	public void AddIngredientToStack(Ingredient ingredient)
    {
        if (ingredientsStack.Contains(ingredient))
            return;

        PhotonView ingredientPhotonView = ingredient.photonView;
        ingredientPhotonView.TransferOwnership(-1);
        photonView.RPC(nameof(AddIngredientToStackRPC), RpcTarget.Others, ingredientPhotonView.ViewID);
        AddIngredientToStackInternal(ingredient);
    }

	[PunRPC]
	private void AddIngredientToStackRPC(int viewID)
    {
		Ingredient ingredient = PhotonView.Find(viewID).GetComponent<Ingredient>();
        AddIngredientToStackInternal(ingredient);
    }

    private void AddIngredientToStackInternal(Ingredient ingredient)
    {
        Debug.Log("Add to stack " + ingredient);
		ingredientsStack.Add(ingredient);
        StackToTop(ingredient.SnapController);
        IngredientAddedEvent?.Invoke(ingredient);
    }

    private void StackToTop(IngredientSnapController snapController)
    {
        snapController.OnSnap(true);
        snapController.SetLastStackCollider(this);
        Ingredient ingredient = snapController.Ingredient;
        ingredient.transform.SetParent(stackContainer);

        float graphicHeight = snapController.GetGraphicHeight();
        Vector3 newSnapPosition = new Vector3(0, totalStackHeight + graphicHeight, 0);
        ingredient.transform.localPosition = newSnapPosition;
        ingredient.transform.localEulerAngles = new Vector3(0, ingredient.transform.eulerAngles.y, 0);
        ingredientHeights.Add(graphicHeight);

        totalStackHeight += graphicHeight;
        snapPoint.SetHeight(totalStackHeight);
    }

    public Ingredient GetTopIngredient()
    {
        if (ingredientsStack == null || ingredientsStack.Count == 0)
            return null;
        else
            return ingredientsStack[ingredientsStack.Count - 1];
    }

    public bool CanRemoveTopIngredient()
    {
        if (ingredientsStack.Count <= 0)
            return false;

        Ingredient ingredient = ingredientsStack[ingredientsStack.Count - 1];
        return ingredient.CanBeGrabbed();
    }

    public Ingredient RemoveTopIngredient()
    {
        Ingredient ingredient = ingredientsStack[ingredientsStack.Count - 1];
        Debug.Log("RemoveTopIngredient: " + ingredient);

        photonView.RPC(nameof(RemoveTopIngredientRPC), RpcTarget.All);
        return ingredient;
    }

    [PunRPC]
	private void RemoveTopIngredientRPC()
    {
        Ingredient ingredient = ingredientsStack[ingredientsStack.Count - 1];
        ingredientsStack.Remove(ingredient);
        ingredient.SnapController.OnSnap(false);
        ingredient.transform.SetParent(null);

        float removedIngredientHeight = ingredientHeights[ingredientHeights.Count - 1];
        ingredientHeights.RemoveAt(ingredientHeights.Count - 1);

        totalStackHeight -= removedIngredientHeight;
        if (totalStackHeight < 0)
            totalStackHeight = 0;
        snapPoint.SetHeight(totalStackHeight);
    }

    public bool ContainsIngredient(IngredientType type)
    {
        foreach (var item in ingredientsStack)
        {
            if (item.IngredientType == type)
                return true;
        }
        return false;
    }

    public override string ToString()
    {
        return base.ToString() + " Ingredients: " + string.Join("-", ingredientsStack);
    }

    public bool CanPlaceSauce(SauceType sauce)
    {
        if (ingredientsStack.Count > 0)
        {
            IngredientType lastIngredient = ingredientsStack[ingredientsStack.Count - 1].IngredientType;
            return !IngredientTypeExtensions.Equals(lastIngredient, sauce) && lastIngredient != IngredientType.BunTop;
        }
        else
        {
            return false;
        }

        return true;
    }

    /// <summary>
    /// Compares order with ingredient stack and returns a result
    /// </summary>
    /// <param name="order"></param>
    /// <returns></returns>
    public OrderDishCompareResult Compare(Order order)
    {
        bool ingredientsAreInCorrectOrder = true;
        float correctIngredientPercentage = 0;
        float properlyCookedIngredientsPercentage = 0;

        int totalIngredientCount = order.ingredients.Length;
        int correctIngredients = 0;
        int properlyCookedIngredients = 0;

        List<IngredientType> dishChecklist = (from i in ingredientsStack select i.IngredientType).ToList();

        int index = 0;
        foreach (IngredientType orderIngredient in order.ingredients)
        {
            // Check for correct order:
            if (ingredientsAreInCorrectOrder && index < dishChecklist.Count && dishChecklist[index] == orderIngredient)
            {
                correctIngredients++;
            }
            // Check for correct ingredients:
            else if (dishChecklist.Contains(orderIngredient))
            {
                // Remove from checklist in case of duplicate ingredients:
                dishChecklist.Remove(orderIngredient);
                correctIngredients++;
                ingredientsAreInCorrectOrder = false;
            }
            else
            {
                ingredientsAreInCorrectOrder = false;
            }

            index++;
        }

        for (int i = 0; i < ingredientsStack.Count; i++)
        {
            Ingredient ingredient = ingredientsStack[i];
            if (ingredient.IsPreparedProperly())
                properlyCookedIngredients++;
        }

        correctIngredientPercentage = Mathf.Lerp(0, 1, (float)correctIngredients / totalIngredientCount);
        properlyCookedIngredientsPercentage = Mathf.Lerp(0, 1, (float)properlyCookedIngredients / totalIngredientCount);

        return new OrderDishCompareResult(ingredientsAreInCorrectOrder, correctIngredientPercentage, properlyCookedIngredientsPercentage);
    }
}
