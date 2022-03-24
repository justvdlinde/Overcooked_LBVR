using Photon.Pun;
using PhysicsCharacter;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dish : MonoBehaviourPun
{
    public List<Ingredient> ingredients = new List<Ingredient>();
    public DishSnapPoint snapPoint = null;

    public bool ContainsIngredient(IngredientType type)
	{
		foreach (var item in ingredients)
		{
			if (item.ingredientType == type)
				return true;
		}
		return false;
	}

	public void AddIngredient(Ingredient ingredient)
    {
        PhotonView ingredientPhotonView = ingredient.rigidbody.GetComponent<PhotonView>();
        ingredientPhotonView.TransferOwnership(-1);
        photonView.RPC(nameof(AddIngredientRPC), RpcTarget.All, ingredientPhotonView.ViewID);
    }

	[PunRPC]
	private void AddIngredientRPC(int viewID)
    {
		Ingredient ingredient = PhotonView.Find(viewID).GetComponentInChildren<Ingredient>();
		ingredients.Add(ingredient);

        Rigidbody ingredientParent = ingredient.rigidbody;
        if (ingredientParent.TryGetComponent(out PhotonRigidbodyView view))
            view.enabled = false;

        ingredientParent.transform.SetParent(snapPoint.ingredientStack);
        Vector3 snapPosition = snapPoint.GetTopSnapPosition(ingredient.processedGraphics.gameObject);
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
        DummyToolHandle handle = ingredient.rigidbody.GetComponentInChildren<DummyToolHandle>(true);
        return handle;
    }

    [PunRPC]
	private void RemoveTopIngredientRPC()
    {
        Ingredient ingredient = ingredients[ingredients.Count - 1];
        ingredients.Remove(ingredient);

        Rigidbody ingredientParent = ingredient.rigidbody;
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
}
