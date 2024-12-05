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

        await _recipeManager.RemoveUnusedRecipes();
        
        // Count amount of recipes
        var count = await _recipeManager.GetAmountOfRecipes();

        var amountToCreate = minAmountInDatabase - count;
        
        await _recipeManager.CreateBatchRandomRecipes(amountToCreate, null);
        
        _logger.LogInformation($"DatabaseJob executed at {DateTime.Now}");
    }
}