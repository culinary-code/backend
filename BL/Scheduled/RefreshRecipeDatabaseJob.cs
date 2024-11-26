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
    
    public Task Execute(IJobExecutionContext context)
    {
        _recipeManager.CreateBatchRandomRecipes(2);
        _logger.LogInformation($"DatabaseJob executed at {DateTime.Now}");
        // Add your database logic here
        return Task.CompletedTask;
    }
}