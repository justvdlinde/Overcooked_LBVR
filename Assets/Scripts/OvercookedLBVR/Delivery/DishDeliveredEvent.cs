using Utils.Core.Events;

public class DishDeliveredEvent : IEvent
{
    public readonly Plate Dish;
    public readonly Order Order;
    public readonly ScoreData Score;

    public DishDeliveredEvent(Plate dish, Order order, ScoreData score)
    {
        Dish = dish;
        Order = order;
        Score = score;
    }
}
