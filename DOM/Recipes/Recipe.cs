using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DOM.Accounts;
using DOM.MealPlanning;
using DOM.Recipes.Ingredients;

namespace DOM.Recipes;

public class Recipe
{
    [Key] 
    public Guid RecipeId { get; set; }
    public string RecipeName { get; set; } = "Default Recipe Name";
    public ICollection<IngredientQuantity> Ingredients { get; set; } = new List<IngredientQuantity>();
    public ICollection<Preference> Preferences { get; set; } = new List<Preference>();
    public RecipeType RecipeType { get; set; }
    public string Description { get; set; } = "Default Description";
    public int CookingTime { get; set; }
    public int AmountOfPeople { get; set; }
    public Difficulty Difficulty { get; set; }
    public string ImagePath { get; set; } = String.Empty;
    public DateTime CreatedAt { get; set; }
    public ICollection<InstructionStep> Instructions { get; set; } = new List<InstructionStep>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
    
    // navigation properties
    public IEnumerable<PlannedMeal> PlannedMeals { get; set; } = new List<PlannedMeal>();
    public IEnumerable<FavoriteRecipe> FavoriteRecipes { get; set; } = new List<FavoriteRecipe>();
}