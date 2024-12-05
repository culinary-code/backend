using System;
using System.ComponentModel.DataAnnotations;
using DOM.Accounts;

namespace DOM.Recipes;

public class FavoriteRecipe
{
    public Guid FavoriteRecipeId { get; set; }
    public Recipe? Recipe { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // navigation properties
    public Account? Account { get; set; }
    
    // Foreign keys
    public Guid? AccountId { get; set; }
    public Guid? RecipeId { get; set; }
}