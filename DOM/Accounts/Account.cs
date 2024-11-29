using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DOM.MealPlanning;
using DOM.Recipes;

namespace DOM.Accounts;

public class Account
{
    [Key] public Guid AccountId { get; set; }
    public string Name { get; set; } = "Default username";
    public string Email { get; set; } = string.Empty;
    public ICollection<Preference> Preferences { get; set; } = new List<Preference>();
    public MealPlanner? Planner { get; set; } = new MealPlanner();
    public ICollection<FavoriteRecipe> FavoriteRecipes { get; set; } = new List<FavoriteRecipe>();
    public int FamilySize { get; set; }
    public GroceryList? GroceryList { get; set; } = new GroceryList();
    public IEnumerable<Review> Reviews { get; set; } = new List<Review>();
    
    //navigation properties
    public Guid? PlannerId { get; set; }
    public Guid? GroceryListId { get; set; }
    
    
}