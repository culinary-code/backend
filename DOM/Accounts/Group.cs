using System.ComponentModel.DataAnnotations;
using DOM.MealPlanning;

namespace DOM.Accounts;

public class Group
{
    [Key] public Guid GroupId { get; set; }
    public string GroupName { get; set; } = "Default groupname";
    public MealPlanner? MealPlanner { get; set; } = new MealPlanner();
    public GroceryList? GroceryList { get; set; } = new GroceryList();
    public ICollection<Account> Accounts { get; set; } = new List<Account>();
    
    // Navigation Property
    public Guid? PlannerId { get; set; }
    public Guid? GroceryListId { get; set; }
}