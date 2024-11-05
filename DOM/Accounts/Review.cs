using System;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using DOM.Recipes;

namespace DOM.Accounts;

public class Review
{
    [Key] public Guid ReviewId { get; set; }
    public Account? Account { get; set; }
    public int AmountOfStars { get; set; }
    public string Description { get; set; } = "Default description";
    public DateTime CreatedAt { get; set; }
    
    // navigation properties
    [JsonIgnore]
    public Recipe? Recipe { get; set; }
}