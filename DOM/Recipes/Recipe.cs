using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DOM.Accounts;
using DOM.Recipes.Ingredients;

namespace DOM.Recipes;

public class Recipe
{
    [Key] 
    public int RecipeId { get; set; }
    public string RecipeName { get; set; } = "Default Recipe Name";
    public IEnumerable<Ingredient> Ingredients { get; set; } = new List<Ingredient>();
    public IEnumerable<Preference> Preferences { get; set; } = new List<Preference>();
    public RecipeType RecipeType { get; set; }
    public string Description { get; set; } = "Default Description";
    public int CookingTime { get; set; }
    public Difficulty Difficulty { get; set; }
    public string ImagePath { get; set; } = String.Empty;
    public DateTime CreatedAt { get; set; }
    public Dictionary<int, string> Instructions { get; set; } = new();
}