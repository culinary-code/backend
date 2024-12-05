using BL.DTOs.Accounts;
using BL.Managers.Recipes;
using DOM.Exceptions;
using Microsoft.Extensions.Logging;

namespace BL.Scheduled;

using Quartz;
using System;
using System.Threading.Tasks;

public class RefreshRecipeDatabaseJob : IJob
{
    private readonly ILogger<RefreshRecipeDatabaseJob> _logger;
    private readonly IRecipeManager _recipeManager;
    
    public RefreshRecipeDatabaseJob(ILogger<RefreshRecipeDatabaseJob> logger, IRecipeManager recipeManager)
    {
        _logger = logger;
        _recipeManager = recipeManager;
    }
    
    public async Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation($"DatabaseJob started at {DateTime.Now}");
        
        var minAmountString = Environment.GetEnvironmentVariable("RECIPE_JOB_MIN_AMOUNT") ?? throw new EnvironmentVariableNotAvailableException("RECIPE_JOB_MIN_AMOUNT environment variable is not set.");
        int minAmountInDatabase = int.Parse(minAmountString);

        await _recipeManager.RemoveUnusedRecipes();
        
        // Count amount of recipes
        var count = await _recipeManager.GetAmountOfRecipes();

        var amountToCreate = minAmountInDatabase - count;
        
        await _recipeManager.CreateBatchRandomRecipes(amountToCreate, null);
        
        _logger.LogInformation($"DatabaseJob executed at {DateTime.Now}");
    }
}