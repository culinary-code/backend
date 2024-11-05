using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using DOM.Accounts;

namespace DOM.Recipes;

public class FavoriteRecipe
{
    [Key] public Guid FavoriteRecipeId { get; set; }
    public Recipe? Recipe { get; set; }
    public DateTime CreatedAt { get; set; }
    
    // navigation properties
    [JsonIgnore]
    public Account? Account { get; set; }
}