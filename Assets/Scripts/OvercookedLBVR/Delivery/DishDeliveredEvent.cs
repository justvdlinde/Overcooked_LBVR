using Utils.Core.Events;

public class DishDeliveredEvent : IEvent
{
    public readonly FoodStack Dish;
    public readonly DeliveryPoint DeliveryPoint;

    public DishDeliveredEvent(FoodStack dish, DeliveryPoint deliveryPoint)
    {
        Dish = dish;
        DeliveryPoint = deliveryPoint;
    }
}
