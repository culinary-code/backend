using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DOM.Accounts;
using DOM.MealPlanning;
using DOM.Recipes.Ingredients;

namespace DOM.Recipes;

public class Recipe
{
    [Key] 
    public int RecipeId { get; set; }
    public string RecipeName { get; set; } = "Default Recipe Name";
    public IEnumerable<IngredientQuantity> Ingredients { get; set; } = new List<IngredientQuantity>();
    public IEnumerable<Preference> Preferences { get; set; } = new List<Preference>();
    public RecipeType RecipeType { get; set; }
    public string Description { get; set; } = "Default Description";
    public int CookingTime { get; set; }
    public Difficulty Difficulty { get; set; }
    public string ImagePath { get; set; } = String.Empty;
    public DateTime CreatedAt { get; set; }
    public IEnumerable<InstructionStep> Instructions { get; set; } = new List<InstructionStep>();
    public IEnumerable<Review> Reviews { get; set; } = new List<Review>();
    
    // navigation properties
    public IEnumerable<PlannedMeal> PlannedMeals { get; set; } = new List<PlannedMeal>();
    public IEnumerable<FavoriteRecipe> FavoriteRecipes { get; set; } = new List<FavoriteRecipe>();
}