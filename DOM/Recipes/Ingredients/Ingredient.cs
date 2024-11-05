using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using DOM.MealPlanning;

namespace DOM.Recipes.Ingredients;

public class Ingredient
{
    [Key]
    public Guid IngredientId { get; set; }
    public string IngredientName { get; set; } = "Default Ingredient";
    public MeasurementType Measurement { get; set; }
    
    // navigation properties
    [JsonIgnore]
    public IEnumerable<IngredientQuantity> IngredientQuantities { get; set; } = new List<IngredientQuantity>();
    
}