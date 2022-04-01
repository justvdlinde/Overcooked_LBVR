public class OrderScoreData
{
    public readonly Order Order;
    public readonly Score Score;

    public OrderScoreData(Order order, Score score)
    {
        Order = order;
        Score = score;
    }
}
