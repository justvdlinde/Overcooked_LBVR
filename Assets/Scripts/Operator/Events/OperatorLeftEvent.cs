using Utils.Core.Events;

public class OperatorLeftEvent : IEvent
{
    public readonly Operator Operator;

    public OperatorLeftEvent(Operator @operator)
    {
        Operator = @operator;
    }
}
