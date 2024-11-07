using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using DOM.Accounts;

namespace DOM.MealPlanning;

public class MealPlanner
{
    [Key] public Guid MealPlannerId { get; set; }
    public IEnumerable<PlannedMeal> NextWeek { get; set; } = new List<PlannedMeal>();
    public IEnumerable<PlannedMeal> History { get; set; } = new List<PlannedMeal>();
    
    // navigation properties
    public Account? Account { get; set; }
}