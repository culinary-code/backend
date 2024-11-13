using DOM.MealPlanning;

namespace DAL.Groceries;

public interface IGroceryRepository
{
    MealPlanner GetMealPlannerById(Guid accountId);
    void CreateGroceryList(GroceryList groceryList);
}