public struct OrderDishCompareResult 
{
    public readonly bool ingredientsAreInCorrectOrder;
    public readonly float correctIngredientPercentage;
    public readonly float properlyCookedIngredientsPercentage;

    public OrderDishCompareResult(bool ingredientsAreInCorrectOrder, float correctIngredientPercentage, float properlyCookedIngredientsPercentage)
    {
        this.ingredientsAreInCorrectOrder = ingredientsAreInCorrectOrder;
        this.correctIngredientPercentage = correctIngredientPercentage;
        this.properlyCookedIngredientsPercentage = properlyCookedIngredientsPercentage;
    }
}
