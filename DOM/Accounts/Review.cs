using System;
using System.ComponentModel.DataAnnotations;
using DOM.Recipes;

namespace DOM.Accounts;

public class Review
{
    [Key] public int ReviewId { get; set; }
    public Recipe? Recipe { get; set; }
    public Account? Account { get; set; }
    public int AmountOfStars { get; set; }
    public string Description { get; set; } = "Default description";
    public DateTime CreatedAt { get; set; }
    //TODO: Username? Had been put in the class diagram but seems unnecessary
}