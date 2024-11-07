namespace BL.DTOs.MealPlanning;

public class MealPlannerDto
{
    public Guid MealPlannerId { get; set; }
    
    public List<PlannedMealDto> NextWeek { get; set; } = new List<PlannedMealDto>();
    
    public List<PlannedMealDto> History { get; set; } = new List<PlannedMealDto>();

    // Navigation property omitted as it is not needed for the DTO
}