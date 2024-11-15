using DOM.MealPlanning;

namespace DAL.Groceries;

public interface IGroceryRepository
{
    GroceryList ReadGroceryListById(Guid id);
    void CreateNewGroceryList(GroceryList groceryList);
    void CreateGroceryList(GroceryList groceryList);
    void UpdateGroceryList(GroceryList groceryList);
}