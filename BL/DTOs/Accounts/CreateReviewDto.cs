using System.ComponentModel.DataAnnotations;

namespace BL.DTOs.Accounts;

public class CreateReviewDto
{
    public Guid RecipeId { get; set; }
    
    public string Description { get; set; } = String.Empty;
    
    [Range(1, 5, ErrorMessage = "Amount of stars must be between 1 and 5")]
    public int AmountOfStars { get; set; }
}