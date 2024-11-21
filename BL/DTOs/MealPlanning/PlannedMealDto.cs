using BL.DTOs.Recipes;
using BL.DTOs.Recipes.Ingredients;

namespace BL.DTOs.MealPlanning;

public class PlannedMealDto
{
    public Guid PlannedMealId { get; set; }
    
    public int AmountOfPeople { get; set; }
    
    public List<IngredientQuantityDto> Ingredients { get; set; } = new List<IngredientQuantityDto>();
    
    public RecipeDto? Recipe { get; set; } // Include RecipeDto to reference the recipe if needed
    
    public DateTime PlannedDate { get; set; }
    
    // Navigation properties omitted as they are not needed for the DTO
}