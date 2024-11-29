using BL.DTOs.Accounts;

namespace BL.DTOs.Recipes;

public class RecipeFilterDto
{
    public List<string> Ingredients { get; set; } = [];
    public string RecipeName { get; set; } = "";
    public string Difficulty { get; set; } = "";
    public string MealType { get; set; } = "";
    public int CookTime { get; set; }
    public List<PreferenceDto> Preferences { get; set; } = new List<PreferenceDto>();
}