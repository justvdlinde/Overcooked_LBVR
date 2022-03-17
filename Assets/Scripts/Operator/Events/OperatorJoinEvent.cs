using Utils.Core.Events;

public class OperatorJoinEvent : IEvent
{
    public readonly Operator Operator;

    public OperatorJoinEvent(Operator @operator)
    {
        Operator = @operator;
    }
}
