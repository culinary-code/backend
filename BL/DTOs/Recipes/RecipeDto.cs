using BL.DTOs.Accounts;
using BL.DTOs.Recipes.Ingredients;
using DOM.Recipes;

namespace BL.DTOs.Recipes;

public class RecipeDto
{
    public Guid RecipeId { get; set; }

    public string RecipeName { get; set; } = "Default Recipe Name";

    public List<IngredientQuantityDto> Ingredients { get; set; } = new List<IngredientQuantityDto>(); // Include DTO for ingredients

    public List<PreferenceDto> Preferences { get; set; } = new List<PreferenceDto>(); // Include DTO for preferences

    public RecipeType RecipeType { get; set; }

    public string Description { get; set; } = "Default Description";

    public int CookingTime { get; set; }

    public Difficulty Difficulty { get; set; }
    
    public int AmountOfPeople { get; set; }

    public string ImagePath { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }
    public DateTime LastUsedAt { get; set; }
    public double AverageRating { get; set; } = 0;
    public int AmountOfRatings { get; set; } = 0;

    public List<InstructionStepDto> Instructions { get; set; } = new List<InstructionStepDto>(); // Include DTO for instruction steps

    public List<ReviewDto> Reviews { get; set; } = new List<ReviewDto>(); // Include DTO for reviews

    // Navigation properties omitted as they are not needed for the DTO
}