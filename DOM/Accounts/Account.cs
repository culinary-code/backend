using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DOM.MealPlanning;
using DOM.Recipes;

namespace DOM.Accounts;

public class Account
{
    [Key] public int AccountId { get; set; }
    public string Name { get; set; } = "Default username";
    public string Email { get; set; } = string.Empty;
    public IEnumerable<Preference> Preferences { get; set; } = new List<Preference>();
    public MealPlanner? Planner { get; set; }
    public IEnumerable<FavoriteRecipe> FavoriteRecipes { get; set; } = new List<FavoriteRecipe>();
    public int FamilySize { get; set; }
    public GroceryList? GroceryList { get; set; }
    public IEnumerable<Review> Reviews { get; set; } = new List<Review>();
}