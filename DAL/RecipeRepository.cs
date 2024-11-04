using DAL.EF;
using DOM.Recipes;

namespace DAL;

public class RecipeRepository : IRecipeRepository
{
    private readonly CulinaryCodeDbContext _ctx;

    public RecipeRepository(CulinaryCodeDbContext ctx)
    {
        _ctx = ctx;
    }

    public Recipe? GetRecipeById(int id)
    {
        throw new System.NotImplementedException();
    }
}