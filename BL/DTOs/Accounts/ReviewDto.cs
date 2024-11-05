using System;

namespace BL.DTOs.Accounts;

public class ReviewDto
{
    public Guid ReviewId { get; set; }

    public Guid? AccountId { get; set; } // To reference the associated account if needed
    
    public Guid? RecipeId { get; set; } // To reference the associated recipe if needed
    
    public int AmountOfStars { get; set; }

    public string Description { get; set; } = "Default description";

    public DateTime CreatedAt { get; set; }

    // Navigation property omitted as it is not needed for the DTO
}