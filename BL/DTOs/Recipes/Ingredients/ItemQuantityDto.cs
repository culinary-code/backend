namespace BL.DTOs.Recipes.Ingredients;

public class ItemQuantityDto
{
    public Guid IngredientQuantityId { get; set; }
    
    public int Quantity { get; set; }

    public IngredientDto? Ingredient { get; set; } // Include IngredientDto for referencing the ingredient if needed

    // Navigation properties omitted as they are not needed for the DTO
}