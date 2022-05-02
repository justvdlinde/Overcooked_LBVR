using Utils.Core.Events;

public class ActiveOrderAddedEvent : IEvent
{
    public readonly Order Order;

    public ActiveOrderAddedEvent(Order order)
    {
        Order = order;
    }
}
