using System.ComponentModel.DataAnnotations;
using DOM.Recipes.Ingredients;

namespace DOM.MealPlanning;

public class GroceryItem
{
    [Key]
    public Guid GroceryItemId { get; set; }
    public string GroceryItemName{ get; set; } = "Default Grocery Item";
        
    // navigation properties
    public IEnumerable<ItemQuantity> ItemQuantities { get; set; } = new List<ItemQuantity>();
}