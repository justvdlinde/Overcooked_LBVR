using System.Collections.Generic;

public class StoryGameScores : IGameModeScoreboard
{
    public List<OrderScoreData> OrderScoreData { get; private set; }

    public StoryGameScores()
    {
        OrderScoreData = new List<OrderScoreData>();
    }

    public void AddScore(OrderScoreData data)
    {
        OrderScoreData.Add(data);
    }

    public override string ToString()
    {
        // Change to return average score + data length
        return base.ToString();
    }

    public void Reset()
    {
        OrderScoreData = new List<OrderScoreData>();
    }

    public void Dispose() { }
}
