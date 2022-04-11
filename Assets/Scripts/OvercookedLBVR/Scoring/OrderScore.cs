public class OrderScore
{
    public readonly DishResult Result;

    public float MaxPoints => 100;
    public float Points { get; private set; }
    public bool IsPerfectScore => Points == MaxPoints;

    public readonly OrderDishCompareResult comparisonResults;

    public OrderScore(float points, DishResult result)
    {
        Points = points;
        Result = result;
    }

    public OrderScore(float points, DishResult result, OrderDishCompareResult comparisonResults) : this(points, result)
    {
        this.comparisonResults = comparisonResults;
    }

    public override string ToString()
    {
        return string.Format("Score: {0}/{1}, correct order: {2}, correct ingredients: {3}% properly cooked ingredients: {4}%",
            Points, MaxPoints, comparisonResults.ingredientsAreInCorrectOrder, comparisonResults.correctIngredientPercentage * 100, comparisonResults.properlyCookedIngredientsPercentage * 100);
    }
}
