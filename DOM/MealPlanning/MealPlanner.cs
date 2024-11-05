using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using DOM.Accounts;

namespace DOM.MealPlanning;

public class MealPlanner
{
    [Key] public int MealPlannerId { get; set; }
    public IEnumerable<PlannedMeal> NextWeek { get; set; } = new List<PlannedMeal>();
    public IEnumerable<PlannedMeal> History { get; set; } = new List<PlannedMeal>();
    
    // navigation properties
    [JsonIgnore]
    public Account? Account { get; set; }
}