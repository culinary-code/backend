using BL.DTOs.MealPlanning;

namespace BL.DTOs.Recipes.Ingredients;

public class ItemQuantityDto
{
    public Guid ItemQuantityId { get; set; }
    
    public float Quantity { get; set; }

    public GroceryItemDto? GroceryItem { get; set; } // Include IngredientDto for referencing the ingredient if needed

    // Navigation properties omitted as they are not needed for the DTO
}