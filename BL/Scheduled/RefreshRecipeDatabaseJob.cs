using BL.Managers.Recipes;
using Configuration.Options;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace BL.Scheduled;

using Quartz;
using System;
using System.Threading.Tasks;

public class RefreshRecipeDatabaseJob : IJob
{
    private readonly JobSettingsOptions _jobSettingsOptions;
    private readonly ILogger<RefreshRecipeDatabaseJob> _logger;
    private readonly IRecipeManager _recipeManager;
    
    public RefreshRecipeDatabaseJob(ILogger<RefreshRecipeDatabaseJob> logger, IRecipeManager recipeManager, IOptions<JobSettingsOptions> jobSettingsOptions)
    {
        _logger = logger;
        _recipeManager = recipeManager;
        _jobSettingsOptions = jobSettingsOptions.Value;
    }
    
    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation($"DatabaseJob started at {DateTime.Now}");
        
        int minAmountInDatabase = _jobSettingsOptions.MinAmount;

        var removeUnusedRecipesResult = await _recipeManager.RemoveUnusedRecipes();
        if (!removeUnusedRecipesResult.IsSuccess)
        {
            _logger.LogError($"Failed to remove unused recipes");
            return;
        }
        
        // Count amount of recipes
        var countResult = await _recipeManager.GetAmountOfRecipes();
        if (!countResult.IsSuccess)
        {
            _logger.LogError($"DatabaseJob failed to get amount from database: {countResult.ErrorMessage}");
            return;
        }
        var count = countResult.Value;

        var amountToCreate = minAmountInDatabase - count;
        
        var createBatchRecipeResult = await _recipeManager.CreateBatchRandomRecipes(amountToCreate, null);
        if (!createBatchRecipeResult.IsSuccess)
        {
            _logger.LogError($"DatabaseJob failed to create batch recipe: {createBatchRecipeResult.ErrorMessage}");
            return;
        }
        
        _logger.LogInformation($"DatabaseJob executed at {DateTime.Now}");
    }
}