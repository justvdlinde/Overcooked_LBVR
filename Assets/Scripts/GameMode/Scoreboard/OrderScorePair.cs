public class OrderScorePair
{
    public readonly Order Order;
    public readonly ScoreData Score;

    public OrderScorePair(Order order, ScoreData score)
    {
        Order = order;
        Score = score;
    }
}
