public class DishFitnessCalculator
{
    public float CalculateFitness(OrderDishCompareResult compareResult, float maxScore = 100)
    {
        float fitScore = maxScore / 3;
        float totalFitness = 0f;
        totalFitness += fitScore * compareResult.correctIngredientPercentage;
        totalFitness += fitScore * compareResult.properlyCookedIngredientsPercentage;
        if (compareResult.ingredientsAreInCorrectOrder)
            totalFitness += fitScore;

        return totalFitness;
    }
}
