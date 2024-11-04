using System;
using System.ComponentModel.DataAnnotations;
using DOM.Accounts;

namespace DOM.Recipes;

public class FavoriteRecipe
{
    [Key] public int FavoriteRecipeId { get; set; }
    public Recipe? Recipe { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // navigation properties
    private Account? Account { get; set; }
}