using System;
using System.Collections.Generic;
using BL.DTOs.Recipes.Ingredients;

namespace BL.DTOs.MealPlanning;

public class GroceryListDto
{
    public Guid GroceryListId { get; set; }
    
    public List<ItemQuantityDto> Items { get; set; } = new List<ItemQuantityDto>();
    
    public List<IngredientQuantityDto> Ingredients { get; set; } = new List<IngredientQuantityDto>();

    // Navigation property omitted as it is not needed for the DTO
}