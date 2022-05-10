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

    public override string ToString()
    {
        return base.ToString() + string.Format(" Correct order: {0}, correct ingredients: {1}, properly cooked: {2}", ingredientsAreInCorrectOrder, correctIngredientPercentage, properlyCookedIngredientsPercentage);
    }
}
