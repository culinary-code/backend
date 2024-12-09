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
    public DbSet<GroceryItem> GroceryItems { get; set; }
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
        // Recipe Entity Configuration
        modelBuilder.Entity<Recipe>(entity =>
        {
            entity.HasKey(r => r.RecipeId);

            entity.HasMany(r => r.Ingredients)
                .WithOne(i => i.Recipe)
                .HasForeignKey(i => i.RecipeId) // explicitly define foreign key
                .OnDelete(DeleteBehavior.Cascade);

            // Many-to-Many relationship with Preferences
            entity.HasMany(a => a.Preferences)
                .WithMany(p => p.Recipes)
                .UsingEntity<RecipePreference>(
                    j => j.HasOne(rp => rp.Preference)
                        .WithMany()
                        .HasForeignKey(ap => ap.PreferenceId),
                    j => j.HasOne(rp => rp.Recipe)
                        .WithMany()
                        .HasForeignKey(ap => ap.RecipeId),
                    j =>
                    {
                        j.HasKey(ap => ap.RecipePreferenceId);
                    });

            entity.HasMany(r => r.Reviews)
                .WithOne(r => r.Recipe)
                .HasForeignKey(r => r.RecipeId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(r => r.Instructions)
                .WithOne(i => i.Recipe)
                .HasForeignKey(i => i.RecipeId)
                .OnDelete(DeleteBehavior.Cascade);
        });


        // PlannedMeal Entity Configuration
        modelBuilder.Entity<PlannedMeal>(entity =>
        {
            entity.HasKey(r => r.PlannedMealId);
            entity.HasOne(p => p.Recipe)
                .WithMany(r => r.PlannedMeals)
                .HasForeignKey(p => p.RecipeId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasMany(p => p.Ingredients)
                .WithOne(i => i.PlannedMeal)
                .HasForeignKey(i => i.PlannedMealId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // MealPlanner Entity Configuration
        modelBuilder.Entity<MealPlanner>(entity =>
        {
            entity.HasKey(r => r.MealPlannerId);
            entity.HasMany(m => m.NextWeek)
                .WithOne(p => p.NextWeekMealPlanner)
                .HasForeignKey(p => p.NextWeekMealPlannerId);

            entity.HasMany(m => m.History)
                .WithOne(p => p.HistoryMealPlanner)
                .HasForeignKey(p => p.HistoryMealPlannerId);
        });

        // Account Entity Configuration
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(r => r.AccountId);
            entity.HasMany(a => a.Reviews)
                .WithOne(r => r.Account)
                .HasForeignKey(r => r.AccountId);

            entity.HasMany(a => a.FavoriteRecipes)
                .WithOne(f => f.Account)
                .HasForeignKey(f => f.AccountId);

            entity.HasOne(a => a.Planner)
                .WithOne(m => m.Account)
                .HasForeignKey<MealPlanner>(m => m.AccountId);

            entity.HasOne(a => a.GroceryList)
                .WithOne(g => g.Account)
                .HasForeignKey<GroceryList>(g => g.AccountId);
            
            // Many-to-Many relationship with Preferences
            entity.HasMany(a => a.Preferences)
                .WithMany(p => p.Accounts)
                .UsingEntity<AccountPreference>(
                    j => j.HasOne(ap => ap.Preference)
                        .WithMany()
                        .HasForeignKey(ap => ap.PreferenceId),
                    j => j.HasOne(ap => ap.Account)
                        .WithMany()
                        .HasForeignKey(ap => ap.AccountId),
                    j =>
                    {
                        j.HasKey(ap => ap.AccountPreferenceId);
                    });
        });

        // GroceryList Entity Configuration
        modelBuilder.Entity<GroceryList>(entity =>
        {
            entity.HasKey(r => r.GroceryListId);
            entity.HasMany(g => g.Ingredients)
                .WithOne(i => i.GroceryList)
                .HasForeignKey(i => i.GroceryListId);

            entity.HasMany(g => g.Items)
                .WithOne(i => i.GroceryList)
                .HasForeignKey(i => i.GroceryListId);
        });

        // IngredientQuantity Entity Configuration
        modelBuilder.Entity<IngredientQuantity>(entity =>
        {
            entity.HasKey(r => r.IngredientQuantityId);
            entity.HasOne(iq => iq.Ingredient)
                .WithMany(i => i.IngredientQuantities)
                .HasForeignKey(iq => iq.IngredientId);
        });

        // ItemQuantity Entity Configuration
        modelBuilder.Entity<ItemQuantity>(entity =>
        {
            entity.HasKey(r => r.ItemQuantityId);
            entity.HasOne(iq => iq.GroceryItem)
                .WithMany(gi => gi.ItemQuantities)
                .HasForeignKey(iq => iq.GroceryItemId);
        });

        // FavoriteRecipe Entity Configuration
        modelBuilder.Entity<FavoriteRecipe>(entity =>
        {
            entity.HasKey(r => r.FavoriteRecipeId);
            entity.HasOne(fr => fr.Recipe)
                .WithMany(r => r.FavoriteRecipes)
                .HasForeignKey(fr => fr.RecipeId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Preference Entity Configuration
        modelBuilder.Entity<Preference>(entity =>
        {
            entity.HasKey(r => r.PreferenceId);
        });
        
        // RecipePreference Entity Configuration
        modelBuilder.Entity<RecipePreference>(entity =>
        {
            entity.HasKey(r => r.RecipePreferenceId);
        });
        
        // RecipePreference Entity Configuration
        modelBuilder.Entity<AccountPreference>(entity =>
        {
            entity.HasKey(r => r.AccountPreferenceId);
        });
        
        // Review Entity Configuration
        modelBuilder.Entity<Review>(entity =>
        {
            entity.HasKey(r => r.ReviewId);
        });
        
        // GroceryItem Entity Configuration
        modelBuilder.Entity<GroceryItem>(entity =>
        {
            entity.HasKey(r => r.GroceryItemId);
        });
        
        // Ingredient Entity Configuration
        modelBuilder.Entity<Ingredient>(entity =>
        {
            entity.HasKey(r => r.IngredientId);
        });
        
        // InstructionStep Entity Configuration
        modelBuilder.Entity<InstructionStep>(entity =>
        {
            entity.HasKey(r => r.InstructionStepId);
        });
        
    }
}