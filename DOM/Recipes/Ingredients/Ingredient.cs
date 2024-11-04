using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace DOM.Recipes.Ingredients;

public class Ingredient
{
    [Key]
    public int IngredientId { get; set; }
    public string IngredientName { get; set; } = "Default Ingredient";
    public MeasurementType Measurement { get; set; }
    
    // navigation properties
    private IEnumerable<Recipe> Recipes { get; set; } = new List<Recipe>();
}