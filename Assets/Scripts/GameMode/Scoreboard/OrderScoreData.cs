public class OrderScoreData
{
    public readonly Order Order;
    public readonly OrderScore Score;

    public OrderScoreData(Order order, OrderScore score)
    {
        Order = order;
        Score = score;
    }
}
