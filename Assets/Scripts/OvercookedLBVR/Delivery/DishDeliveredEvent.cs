using Utils.Core.Events;

public class DishDeliveredEvent : IEvent
{
    public readonly Dish Dish;
    public readonly DeliveryPoint DeliveryPoint;

    public DishDeliveredEvent(Dish dish, DeliveryPoint deliveryPoint)
    {
        Dish = dish;
        DeliveryPoint = deliveryPoint;
    }
}
