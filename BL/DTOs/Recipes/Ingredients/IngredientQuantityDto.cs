namespace BL.DTOs.Recipes.Ingredients;

public class IngredientQuantityDto
{
    
    public Guid IngredientQuantityId { get; set; }
    
    public float Quantity { get; set; }

    public IngredientDto? Ingredient { get; set; }

    public string RecipeName { get; set; } = "";

    // Navigation properties omitted as they are not needed for the DTO
}