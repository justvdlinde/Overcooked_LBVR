public class ScoreCalculator
{
    public float maxScore = 100;

    public virtual OrderScore CalculateScore(Order order, FoodStack dish, DishResult result)
    {
        if (result == DishResult.TimerExceeded)
            return new OrderScore(0, result);

        OrderDishCompareResult comparisonResult = dish.Compare(order);

        float pointSegment = maxScore / 3;
        float totalPoints = 0f;
        totalPoints += pointSegment * comparisonResult.correctIngredientPercentage;
        totalPoints += pointSegment * comparisonResult.properlyCookedIngredientsPercentage;
        if (comparisonResult.ingredientsAreInCorrectOrder)
            totalPoints += pointSegment;

        return new OrderScore(totalPoints, result, comparisonResult);
    }
}
