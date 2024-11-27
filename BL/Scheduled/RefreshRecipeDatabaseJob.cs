using BL.Managers.Recipes;
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
        int minAmountInDatabase = 100;
        // TODO: add clearing of unused recipes here (other issue)
        
        // Count amount of recipes
        var count = await _recipeManager.GetAmountOfRecipesAsync();

        var amountToCreate = minAmountInDatabase - count;
        await _recipeManager.CreateBatchRandomRecipes(amountToCreate);
        
        _logger.LogInformation($"DatabaseJob executed at {DateTime.Now}");
    }
}