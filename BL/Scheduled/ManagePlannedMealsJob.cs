using BL.Managers.MealPlanning;
using DAL.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Quartz;

namespace BL.Scheduled;

public class ManagePlannedMealsJob : IJob
{
    private readonly IMealPlannerManager _mealPlannerManager;
    private readonly ILogger<ManagePlannedMealsJob> _logger;

    public ManagePlannedMealsJob(IMealPlannerManager mealPlannerManager, ILogger<ManagePlannedMealsJob> logger)
    {
        _mealPlannerManager = mealPlannerManager;
        _logger = logger;
    }

    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation($"DatabaseJob Removing old Planned Meals started at {DateTime.Now}");
        var result = await _mealPlannerManager.MoveAndRemoveOldPlannedMeals();
        if (!result.IsSuccess)
        {
            _logger.LogError($"DatabaseJob Removing old Planned Meals failed to get amount from database: {result.ErrorMessage}");
            return;
        }
        _logger.LogInformation($"DatabaseJob Removing old Planned Meals executed at {DateTime.Now}");

    }
}