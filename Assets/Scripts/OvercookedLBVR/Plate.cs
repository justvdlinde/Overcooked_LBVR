using Photon.Pun;
using UnityEngine;

[SelectionBase]
public class Plate : MonoBehaviour
{
    public FoodStack FoodStack => foodStack;

    [SerializeField] private FoodStack foodStack = null;

    public void OnDeliver()
    {
        // TODO: instantiate some kind of particle/feedback
        PhotonNetwork.Destroy(gameObject);
    }

    public bool CanBeDelivered()
    {
        return (foodStack != null && foodStack.IngredientsStack.Count >= DeliveryPoint.DISH_MIN_INGREDIENTS);
    }
}
