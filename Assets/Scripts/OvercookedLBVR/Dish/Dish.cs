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

public class Dish : MonoBehaviourPun
{
    public List<Ingredient> ingredients = new List<Ingredient>();
    public DishSnapPoint snapPoint = null;

    public bool ContainsIngredient(IngredientType type)
	{
		foreach (var item in ingredients)
		{
			if (item.IngredientType == type)
				return true;
		}
		return false;
	}

	public void AddIngredient(Ingredient ingredient)
    {
        PhotonView ingredientPhotonView = ingredient.GetComponent<Rigidbody>().GetComponent<PhotonView>();
        ingredientPhotonView.TransferOwnership(-1);
        photonView.RPC(nameof(AddIngredientRPC), RpcTarget.All, ingredientPhotonView.ViewID);
    }

	[PunRPC]
	private void AddIngredientRPC(int viewID)
    {
		Ingredient ingredient = PhotonView.Find(viewID).GetComponentInChildren<Ingredient>();
		ingredients.Add(ingredient);

        Rigidbody ingredientParent = ingredient.GetComponent<Rigidbody>();
        if (ingredientParent.TryGetComponent(out PhotonRigidbodyView view))
            view.enabled = false;

        ingredientParent.transform.SetParent(snapPoint.ingredientStack);
        Vector3 snapPosition = snapPoint.GetTopSnapPosition(ingredient.gameObject); //ingredient.processedGraphics.gameObject
        snapPoint.stackElements.Add(snapPosition.y);

        float diff = snapPosition.y - snapPoint.totalStackHeight;
        snapPoint.totalStackHeight = snapPosition.y;
        snapPosition.y -= diff * 0.5f;

        ingredientParent.transform.localPosition = snapPosition;
        ingredientParent.transform.localEulerAngles = new Vector3(0, ingredientParent.transform.eulerAngles.y, 0);
        snapPoint.UpdateTriggerPosition();
        //UpdateTriggerSize();

        Collider[] colliders = ingredientParent.GetComponentsInChildren<Collider>(true);
        foreach (Collider collider in colliders)
        {
            if (!collider.isTrigger)
                collider.enabled = false;
        }

        ingredientParent.velocity = Vector3.zero;
        ingredientParent.isKinematic = true;
        ingredientParent.useGravity = false;
        Tool t = ingredientParent.GetComponent<Tool>();
        if (t != null)
            ingredientParent.GetComponent<Tool>().enabled = false;

        ingredient.SetComponentsOnIngredientActive(false);
    }

	public DummyToolHandle RemoveTopIngredient(Ingredient ingredient)
    {
        if (!ingredients.Contains(ingredient))
            return null;

        photonView.RPC(nameof(RemoveTopIngredientRPC), RpcTarget.All);
        DummyToolHandle handle = ingredient.GetComponent<Rigidbody>().GetComponentInChildren<DummyToolHandle>(true);
        return handle;
    }

    [PunRPC]
	private void RemoveTopIngredientRPC()
    {
        Ingredient ingredient = ingredients[ingredients.Count - 1];
        ingredients.Remove(ingredient);

        Rigidbody ingredientParent = ingredient.GetComponent<Rigidbody>();
        if (ingredientParent.TryGetComponent(out PhotonRigidbodyView view))
            view.enabled = true;

        ingredientParent.isKinematic = false;
        ingredientParent.useGravity = true;
        ingredientParent.transform.parent = null;
        Tool t = ingredientParent.GetComponent<Tool>();
        if (t != null)
            t.enabled = true;


        ingredient.SetComponentsOnIngredientActive(true);

        Collider[] colliders = ingredientParent.GetComponentsInChildren<Collider>(true);
        foreach (Collider collider in colliders)
            collider.enabled = true;

        if (ingredients.Contains(ingredient))
            ingredients.Remove(ingredient);

        ingredient.CanStack = false;
        ingredient.recentDishCollider = transform;

        snapPoint.RecomputeStackHeight();
        snapPoint.UpdateTriggerPosition();
        //UpdateTriggerSize();
    }

    public void OnDeliver()
    {
        // TODO: check dish hierarchy for which object to destroy
        // TODO: instantiate some kind of particle/feedback
        PhotonNetwork.Destroy(gameObject);
    }

    public override string ToString()
    {
        return base.ToString() + " Ingredients: " + string.Join("-", ingredients);
    }

    public OrderDishCompareResult Compare(Order order)
    {
        bool ingredientsAreInCorrectOrder = true;
        float correctIngredientPercentage = 0;
        float properlyCookedIngredientsPercentage = 0;

        int totalIngredientCount = order.ingredients.Length;
        int correctIngredients = 0;
        int properlyCookedIngredients = 0;

        List<IngredientType> dishChecklist = (from i in ingredients select i.IngredientType).ToList();

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

        for (int i = 0; i < ingredients.Count; i++)
        {
            Ingredient ingredient = ingredients[i];
            if (ingredient.IsPreparedProperly())
                properlyCookedIngredients++;
        }

        correctIngredientPercentage = Mathf.Lerp(0, 1, (float)correctIngredients / totalIngredientCount);
        properlyCookedIngredientsPercentage = Mathf.Lerp(0, 1, (float)properlyCookedIngredients / totalIngredientCount);

        return new OrderDishCompareResult(ingredientsAreInCorrectOrder, correctIngredientPercentage, properlyCookedIngredientsPercentage);
    }
}
