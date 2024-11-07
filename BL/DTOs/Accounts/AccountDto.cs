using BL.DTOs.MealPlanning;
using BL.DTOs.Recipes;

namespace BL.DTOs.Accounts;

public class AccountDto
{
    public Guid AccountId { get; set; }
    
    public string Name { get; set; } = "Default username";
    
    public string Email { get; set; } = string.Empty;
    
    public List<PreferenceDto> Preferences { get; set; } = new List<PreferenceDto>();
    
    public MealPlannerDto? Planner { get; set; }
    
    public List<FavoriteRecipeDto> FavoriteRecipes { get; set; } = new List<FavoriteRecipeDto>();
    
    public int FamilySize { get; set; }
    
    public GroceryListDto? GroceryList { get; set; }
    
    public List<ReviewDto> Reviews { get; set; } = new List<ReviewDto>();

    
}