using Photon.Pun;
using PhysicsCharacter;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public enum DishResult
{
    None,
    Delivered,
    TimerExceeded
}

public class FoodStack : MonoBehaviourPun
{
    public List<Ingredient> ingredientsStack = new List<Ingredient>();
    public FoodStackSnapPoint snapPoint = null;

	public void AddIngredientToStack(Ingredient ingredient)
    {
        PhotonView ingredientPhotonView = ingredient.photonView;
        ingredientPhotonView.TransferOwnership(-1);
        photonView.RPC(nameof(AddIngredientToStackRPC), RpcTarget.All, ingredientPhotonView.ViewID);
    }

	[PunRPC]
	private void AddIngredientToStackRPC(int viewID)
    {
		Ingredient ingredient = PhotonView.Find(viewID).GetComponent<Ingredient>();
		ingredientsStack.Add(ingredient);
        Snap(ingredient.SnapController);
    }

    private void Snap(IngredientSnapController snapController)
    {
        snapController.OnSnap(true);
        snapController.Ingredient.transform.SetParent(snapPoint.ingredientStack);

        Vector3 snapPosition = snapPoint.GetSnapPosition(snapController); 
        snapPoint.stackElements.Add(snapPosition.y);

        float diff = snapPosition.y - snapPoint.totalStackHeight;
        snapPoint.totalStackHeight = snapPosition.y;
        snapPosition.y -= diff * 0.5f; // ??

        snapController.transform.localPosition = snapPosition;
        snapController.transform.localEulerAngles = new Vector3(0, snapController.transform.eulerAngles.y, 0);
        snapPoint.SetPositionToStackEnd();
    }

    public Ingredient GetTopIngredient()
    {
        if (ingredientsStack == null || ingredientsStack.Count == 0)
            return null;
        else
            return ingredientsStack[ingredientsStack.Count - 1];
    }

	public void RemoveTopIngredient()
    {
        photonView.RPC(nameof(RemoveTopIngredientRPC), RpcTarget.All);
    }

    [PunRPC]
	private void RemoveTopIngredientRPC()
    {
        Ingredient ingredient = ingredientsStack[ingredientsStack.Count - 1];
        ingredientsStack.Remove(ingredient);
        ingredient.SnapController.OnSnap(false);

        if (ingredientsStack.Contains(ingredient))
            ingredientsStack.Remove(ingredient);

        // TODO: cleanup
        ingredient.SnapController.canStack = false;
        ingredient.SnapController.recentDishCollider = transform;

        // TODO: do this in this class
        snapPoint.RecomputeStackHeight();
        snapPoint.SetPositionToStackEnd();
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

    // Move into seperate plate class?
    public void OnDeliver()
    {
        // TODO: check dish hierarchy for which object to destroy
        // TODO: instantiate some kind of particle/feedback
        PhotonNetwork.Destroy(gameObject);
    }

    public override string ToString()
    {
        return base.ToString() + " Ingredients: " + string.Join("-", ingredientsStack);
    }

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
