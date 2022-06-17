using UnityEngine;
using Utils.Core.Attributes;

public class OrderGeneratorTester : MonoBehaviour
{
    [SerializeField] private OrdersController ordersController = null;
    [SerializeField] private int orderTier = 0;

    private TieredOrderGenerator orderGenerator;

    private void Start()
    {
        orderGenerator = new TieredOrderGenerator(true);
    }

    [Button]
    private void UpdateGeneratorSettings()
    {
        orderGenerator.currentTier = orderTier;
    }

    [Button]
    public void CreateNewActiveOrder()
    {
        Order order = orderGenerator.Generate();
        order.orderIndex = 0;
        ordersController.AddActiveOrder(order);
    }
}
