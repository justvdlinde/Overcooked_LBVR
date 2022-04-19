using Utils.Core.Events;

public class DishDeliveredEvent : IEvent
{
    public readonly Plate Dish;
    public readonly DeliveryPoint DeliveryPoint;

    public DishDeliveredEvent(Plate dish, DeliveryPoint deliveryPoint)
    {
        Dish = dish;
        DeliveryPoint = deliveryPoint;
    }
}
