﻿using System;
using System.ComponentModel.DataAnnotations;
using DOM.Recipes;

namespace DOM.Accounts;

public class Review
{
    public Guid ReviewId { get; set; }
    public Account? Account { get; set; }
    public int AmountOfStars { get; set; }
    public string Description { get; set; } = "Default description";
    public DateTime CreatedAt { get; set; }
    
    // navigation properties
    public Recipe? Recipe { get; set; }
    
    // Foreign keys
    public Guid? RecipeId { get; set; }
    public Guid? AccountId { get; set; }
}