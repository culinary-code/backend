using System;

namespace BL.DTOs.Recipes;

public class FavoriteRecipeDto
{
    public Guid FavoriteRecipeId { get; set; }
    
    public RecipeDto? Recipe { get; set; } // Include RecipeDto to reference the recipe if needed
    
    public DateTime CreatedAt { get; set; }

    // Navigation properties omitted as they are not needed for the DTO
}