using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DOM.MealPlanning;

namespace DOM.Recipes.Ingredients;

public class Ingredient
{
    [Key]
    public int IngredientId { get; set; }
    public string IngredientName { get; set; } = "Default Ingredient";
    public MeasurementType Measurement { get; set; }
    
    // navigation properties
    public IEnumerable<IngredientQuantity> IngredientQuantities { get; set; } = new List<IngredientQuantity>();
    
}