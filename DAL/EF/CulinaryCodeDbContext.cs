using System.Diagnostics;
using DOM.Accounts;
using DOM.MealPlanning;
using DOM.Recipes;
using DOM.Recipes.Ingredients;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace DAL.EF;

public class CulinaryCodeDbContext : DbContext
{
    public DbSet<Account> Accounts { get; set; }
    public DbSet<Preference> Preferences { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<GroceryList> GroceryLists { get; set; }
    public DbSet<MealPlanner> MealPlanners { get; set; }
    public DbSet<PlannedMeal> PlannedMeals { get; set; }
    public DbSet<Ingredient> Ingredients { get; set; }
    public DbSet<FavoriteRecipe> FavoriteRecipes { get; set; }
    public DbSet<Recipe> Recipes { get; set; }
    public DbSet<InstructionStep> InstructionSteps { get; set; }
    public DbSet<IngredientQuantity> IngredientQuantities { get; set; }
    public DbSet<ItemQuantity> ItemQuantities { get; set; }

    public CulinaryCodeDbContext(DbContextOptions options) : base(options)
    {
        CulinaryCodeDbInitializer.Initialize(this, dropCreateDatabase: true);
    }


    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        // configure logging: write to debug output window
        optionsBuilder.LogTo(message => Debug.WriteLine(message), LogLevel.Information);
        
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Recipe>()
            .HasMany(r => r.Ingredients)
            .WithOne(i => i.Recipe);
        
        //TODO: many to many relationship, make our own class definition?
        modelBuilder.Entity<Recipe>()
            .HasMany(r => r.Preferences)
            .WithMany(p => p.Recipes);

        modelBuilder.Entity<Recipe>()
            .HasMany(r => r.Reviews)
            .WithOne(r => r.Recipe);
        
        modelBuilder.Entity<Recipe>()
            .HasMany(r => r.Instructions)
            .WithOne(i => i.Recipe);

        modelBuilder.Entity<PlannedMeal>()
            .HasOne(p => p.Recipe)
            .WithMany(r => r.PlannedMeals);
        
        modelBuilder.Entity<PlannedMeal>()
            .HasMany(p => p.Ingredients)
            .WithOne(i => i.PlannedMeal);
        
        modelBuilder.Entity<MealPlanner>()
            .HasMany(m => m.NextWeek)
            .WithOne(p => p.NextWeekMealPlanner);
        
        modelBuilder.Entity<MealPlanner>()
            .HasMany(m => m.History)
            .WithOne(p => p.HistoryMealPlanner);
        
        modelBuilder.Entity<Account>()
            .HasMany(a => a.Reviews)
            .WithOne(r => r.Account);
        
        modelBuilder.Entity<Account>()
            .HasMany(a => a.FavoriteRecipes)
            .WithOne(f => f.Account);

        modelBuilder.Entity<Account>()
            .HasOne(a => a.Planner)
            .WithOne(m => m.Account)
            .HasForeignKey<Account>(a => a.PlannerId);
        
        modelBuilder.Entity<Account>()
            .HasOne(a => a.GroceryList)
            .WithOne(g => g.Account)
            .HasForeignKey<Account>(a => a.GroceryListId);
        
        modelBuilder.Entity<GroceryList>()
            .HasMany(g => g.Ingredients)
            .WithOne(i => i.GroceryList);
        
        modelBuilder.Entity<GroceryList>()
            .HasMany(g => g.Items)
            .WithOne(i => i.GroceryList);
        
        modelBuilder.Entity<IngredientQuantity>()
            .HasOne(i => i.Ingredient)
            .WithMany(i => i.IngredientQuantities);
        
        modelBuilder.Entity<FavoriteRecipe>()
            .HasOne(f => f.Recipe)
            .WithMany(r => r.FavoriteRecipes);
    }
}