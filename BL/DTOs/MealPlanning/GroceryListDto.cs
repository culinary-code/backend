using BL.DTOs.Accounts;
using BL.DTOs.Recipes.Ingredients;
using DOM.Recipes.Ingredients;

namespace BL.DTOs.MealPlanning;

public class GroceryListDto
{
    public Guid GroceryListId { get; set; }
    
    public List<ItemQuantityDto> Items { get; set; } = new List<ItemQuantityDto>();
    
    public AccountDto Account { get; set; }

    public List<IngredientQuantity> Ingredients { get; set; } = new List<IngredientQuantity>();

    // Navigation property omitted as it is not needed for the DTO
}