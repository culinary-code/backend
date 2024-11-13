using DOM.MealPlanning;

namespace DAL.Groceries;

public interface IGroceryRepository
{
    void CreateGroceryList(GroceryList groceryList);
}