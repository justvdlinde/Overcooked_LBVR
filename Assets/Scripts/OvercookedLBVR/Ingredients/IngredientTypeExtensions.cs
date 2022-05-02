public static class IngredientTypeExtensions
{
    public static bool Equals(this IngredientType ingredient, SauceType sauce)
    {
        if (sauce == SauceType.Ketchup)
            return ingredient == IngredientType.Ketchup;
        else
            return ingredient == IngredientType.Mayo;
    }
}
