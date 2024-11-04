using System.ComponentModel.DataAnnotations;
using DOM.MealPlanning;

namespace DOM.Recipes.Ingredients;

public class ItemQuantity
{
    [Key]
    public int IngredientQuantityId { get; set; }
    public int Quantity { get; set; }
    public Ingredient? Ingredient { get; set; } 
    
    // navigation properties
    public GroceryList? GroceryList { get; set; }
}