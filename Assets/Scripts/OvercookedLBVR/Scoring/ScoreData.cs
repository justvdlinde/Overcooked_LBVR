public class ScoreData
{
    public readonly DishResult Result;

    public const float MaxPoints = 100;
    public float Points { get; private set; }
    public bool IsPerfectScore => Points == MaxPoints;

    public readonly OrderDishCompareResult comparisonResult;

    public ScoreData(float points, DishResult result)
    {
        Points = points;
        Result = result;
    }

    public ScoreData(float points, DishResult result, OrderDishCompareResult comparisonResults) : this(points, result)
    {
        this.comparisonResult = comparisonResults;
    }

    public override string ToString()
    {
        return string.Format("Score: {0}/{1}, correct order: {2}, correct ingredients: {3}% properly cooked ingredients: {4}%",
            Points, MaxPoints, comparisonResult.ingredientsAreInCorrectOrder, comparisonResult.correctIngredientPercentage * 100, comparisonResult.properlyCookedIngredientsPercentage * 100);
    }
}
