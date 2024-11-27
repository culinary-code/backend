using DOM.Recipes.Ingredients;

namespace BL.DTOs.MealPlanning;

public class GroceryItemDto
{
    public Guid GroceryItemId { get; set; }
    public string GroceryItemName{ get; set; } = "Default Grocery Item";
    public MeasurementType Measurement { get; set; }
}