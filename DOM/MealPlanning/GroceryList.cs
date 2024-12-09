using System.ComponentModel.DataAnnotations;
using DOM.Accounts;
using DOM.Recipes.Ingredients;

namespace DOM.MealPlanning;

public class GroceryList
{
    public Guid GroceryListId { get; set; }
    public ICollection<ItemQuantity> Items { get; set; } = new List<ItemQuantity>();
    public ICollection<IngredientQuantity> Ingredients { get; set; } = new List<IngredientQuantity>();
    
    // navigation properties
    public Account? Account { get; set; } 
    public Group? Group { get; set; }
    
    // Foreign keys
    public Guid? AccountId { get; set; }
}