using DOM.Recipes.Ingredients;

namespace BL.DTOs.Recipes.Ingredients;

public class IngredientDto
{
    public Guid IngredientId { get; set; }
    
    public string IngredientName { get; set; } = "Default Ingredient";

    public MeasurementType Measurement { get; set; }

    // Navigation property omitted as it is not needed for the DTO
}