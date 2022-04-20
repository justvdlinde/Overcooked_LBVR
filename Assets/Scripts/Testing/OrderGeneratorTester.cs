using UnityEngine;
using Utils.Core.Attributes;

public class OrderGeneratorTester : MonoBehaviour
{
    [SerializeField] private OrdersController ordersController = null;
    
    // TODO: make this changeable using buttons and UI in scene
    [SerializeField] private int orderTier = 3;
    [SerializeField] private float orderDuration = 30;

    private TieredOrderGenerator orderGenerator;

    private void Start()
    {
        orderGenerator = new TieredOrderGenerator();
    }

    [Button]
    public void CreateNewActiveOrder()
    {
        Order order = orderGenerator.GenerateRandomOrder(orderTier, 0, out int newTier, true);
        order.orderNumber = 0;
        order.timer.Set(orderDuration); 
        ordersController.AddActiveOrder(order);
    }
}
