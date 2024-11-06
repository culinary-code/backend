using DOM.Recipes.Ingredients;

namespace DAL.Recipes;

public interface IIngredientRepository
{
    public Ingredient ReadIngredientById(Guid id);
    public Ingredient ReadIngredientByName(string name);
    public void CreateIngredient(Ingredient ingredient);
    public void UpdateIngredient(Ingredient ingredient);
}