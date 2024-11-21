using DOM.MealPlanning;

namespace DAL.Groceries;

public interface IMealPlannerRepository
{ 
    MealPlanner ReadMealPlannerById(Guid accountId);
}