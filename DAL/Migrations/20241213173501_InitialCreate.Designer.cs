﻿// <auto-generated />
using System;
using DAL.EF;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace DAL.Migrations
{
    [DbContext(typeof(CulinaryCodeDbContext))]
    [Migration("20241213173501_InitialCreate")]
    partial class InitialCreate
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "8.0.10")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("AccountGroup", b =>
                {
                    b.Property<Guid>("AccountsAccountId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("GroupsGroupId")
                        .HasColumnType("uuid");

                    b.HasKey("AccountsAccountId", "GroupsGroupId");

                    b.HasIndex("GroupsGroupId");

                    b.ToTable("AccountGroup");
                });

            modelBuilder.Entity("DOM.Accounts.Account", b =>
                {
                    b.Property<Guid>("AccountId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid?>("ChosenGroupId")
                        .HasColumnType("uuid");

                    b.Property<string>("Email")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<int>("FamilySize")
                        .HasColumnType("integer");

                    b.Property<Guid?>("GroceryListId")
                        .HasColumnType("uuid");

                    b.Property<string>("Name")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid?>("PlannerId")
                        .HasColumnType("uuid");

                    b.HasKey("AccountId");

                    b.ToTable("Accounts");
                });

            modelBuilder.Entity("DOM.Accounts.AccountPreference", b =>
                {
                    b.Property<Guid>("AccountPreferenceId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("AccountId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("PreferenceId")
                        .HasColumnType("uuid");

                    b.HasKey("AccountPreferenceId");

                    b.HasIndex("AccountId");

                    b.HasIndex("PreferenceId");

                    b.ToTable("AccountPreferences");
                });

            modelBuilder.Entity("DOM.Accounts.Group", b =>
                {
                    b.Property<Guid>("GroupId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid?>("GroceryListId")
                        .HasColumnType("uuid");

                    b.Property<string>("GroupName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid?>("PlannerId")
                        .HasColumnType("uuid");

                    b.HasKey("GroupId");

                    b.HasIndex("GroceryListId")
                        .IsUnique();

                    b.HasIndex("PlannerId")
                        .IsUnique();

                    b.ToTable("Groups");
                });

            modelBuilder.Entity("DOM.Accounts.Invitation", b =>
                {
                    b.Property<Guid>("InvitationId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<DateTime?>("AcceptedDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<DateTime>("ExpirationDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid>("GroupId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("InviterId")
                        .HasColumnType("uuid");

                    b.Property<string>("InviterName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<string>("Token")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<bool>("isAccepted")
                        .HasColumnType("boolean");

                    b.HasKey("InvitationId");

                    b.HasIndex("GroupId");

                    b.HasIndex("InviterId");

                    b.ToTable("Invitations");
                });

            modelBuilder.Entity("DOM.Accounts.Preference", b =>
                {
                    b.Property<Guid>("PreferenceId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("PreferenceName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<bool>("StandardPreference")
                        .HasColumnType("boolean");

                    b.HasKey("PreferenceId");

                    b.ToTable("Preferences");
                });

            modelBuilder.Entity("DOM.Accounts.Review", b =>
                {
                    b.Property<Guid>("ReviewId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid?>("AccountId")
                        .HasColumnType("uuid");

                    b.Property<int>("AmountOfStars")
                        .HasColumnType("integer");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid?>("RecipeId")
                        .HasColumnType("uuid");

                    b.HasKey("ReviewId");

                    b.HasIndex("AccountId");

                    b.HasIndex("RecipeId");

                    b.ToTable("Reviews");
                });

            modelBuilder.Entity("DOM.MealPlanning.GroceryItem", b =>
                {
                    b.Property<Guid>("GroceryItemId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("GroceryItemName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<byte>("Measurement")
                        .HasColumnType("smallint");

                    b.HasKey("GroceryItemId");

                    b.ToTable("GroceryItems");
                });

            modelBuilder.Entity("DOM.MealPlanning.GroceryList", b =>
                {
                    b.Property<Guid>("GroceryListId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid?>("AccountId")
                        .HasColumnType("uuid");

                    b.HasKey("GroceryListId");

                    b.HasIndex("AccountId")
                        .IsUnique();

                    b.ToTable("GroceryLists");
                });

            modelBuilder.Entity("DOM.MealPlanning.MealPlanner", b =>
                {
                    b.Property<Guid>("MealPlannerId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid?>("AccountId")
                        .HasColumnType("uuid");

                    b.HasKey("MealPlannerId");

                    b.HasIndex("AccountId")
                        .IsUnique();

                    b.ToTable("MealPlanners");
                });

            modelBuilder.Entity("DOM.MealPlanning.PlannedMeal", b =>
                {
                    b.Property<Guid>("PlannedMealId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<int>("AmountOfPeople")
                        .HasColumnType("integer");

                    b.Property<Guid?>("HistoryMealPlannerId")
                        .HasColumnType("uuid");

                    b.Property<Guid?>("NextWeekMealPlannerId")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("PlannedDate")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid?>("RecipeId")
                        .HasColumnType("uuid");

                    b.HasKey("PlannedMealId");

                    b.HasIndex("HistoryMealPlannerId");

                    b.HasIndex("NextWeekMealPlannerId");

                    b.HasIndex("RecipeId");

                    b.ToTable("PlannedMeals");
                });

            modelBuilder.Entity("DOM.Recipes.FavoriteRecipe", b =>
                {
                    b.Property<Guid>("FavoriteRecipeId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid?>("AccountId")
                        .HasColumnType("uuid");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<Guid?>("RecipeId")
                        .HasColumnType("uuid");

                    b.HasKey("FavoriteRecipeId");

                    b.HasIndex("AccountId");

                    b.HasIndex("RecipeId");

                    b.ToTable("FavoriteRecipes");
                });

            modelBuilder.Entity("DOM.Recipes.Ingredients.Ingredient", b =>
                {
                    b.Property<Guid>("IngredientId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("IngredientName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<byte>("Measurement")
                        .HasColumnType("smallint");

                    b.HasKey("IngredientId");

                    b.ToTable("Ingredients");
                });

            modelBuilder.Entity("DOM.Recipes.Ingredients.IngredientQuantity", b =>
                {
                    b.Property<Guid>("IngredientQuantityId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid?>("GroceryListId")
                        .HasColumnType("uuid");

                    b.Property<Guid?>("IngredientId")
                        .HasColumnType("uuid");

                    b.Property<Guid?>("PlannedMealId")
                        .HasColumnType("uuid");

                    b.Property<float>("Quantity")
                        .HasColumnType("real");

                    b.Property<Guid?>("RecipeId")
                        .HasColumnType("uuid");

                    b.HasKey("IngredientQuantityId");

                    b.HasIndex("GroceryListId");

                    b.HasIndex("IngredientId");

                    b.HasIndex("PlannedMealId");

                    b.HasIndex("RecipeId");

                    b.ToTable("IngredientQuantities");
                });

            modelBuilder.Entity("DOM.Recipes.Ingredients.ItemQuantity", b =>
                {
                    b.Property<Guid>("ItemQuantityId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid?>("GroceryItemId")
                        .HasColumnType("uuid");

                    b.Property<Guid?>("GroceryListId")
                        .HasColumnType("uuid");

                    b.Property<float>("Quantity")
                        .HasColumnType("real");

                    b.HasKey("ItemQuantityId");

                    b.HasIndex("GroceryItemId");

                    b.HasIndex("GroceryListId");

                    b.ToTable("ItemQuantities");
                });

            modelBuilder.Entity("DOM.Recipes.InstructionStep", b =>
                {
                    b.Property<Guid>("InstructionStepId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<string>("Instruction")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<Guid?>("RecipeId")
                        .HasColumnType("uuid");

                    b.Property<int>("StepNumber")
                        .HasColumnType("integer");

                    b.HasKey("InstructionStepId");

                    b.HasIndex("RecipeId");

                    b.ToTable("InstructionSteps");
                });

            modelBuilder.Entity("DOM.Recipes.Recipe", b =>
                {
                    b.Property<Guid>("RecipeId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<int>("AmountOfPeople")
                        .HasColumnType("integer");

                    b.Property<int>("AmountOfRatings")
                        .HasColumnType("integer");

                    b.Property<double>("AverageRating")
                        .HasColumnType("double precision");

                    b.Property<int>("CookingTime")
                        .HasColumnType("integer");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("Description")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<byte>("Difficulty")
                        .HasColumnType("smallint");

                    b.Property<string>("ImagePath")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<DateTime>("LastUsedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<string>("RecipeName")
                        .IsRequired()
                        .HasColumnType("text");

                    b.Property<byte>("RecipeType")
                        .HasColumnType("smallint");

                    b.HasKey("RecipeId");

                    b.ToTable("Recipes");
                });

            modelBuilder.Entity("DOM.Recipes.RecipePreference", b =>
                {
                    b.Property<Guid>("RecipePreferenceId")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("uuid");

                    b.Property<Guid>("PreferenceId")
                        .HasColumnType("uuid");

                    b.Property<Guid>("RecipeId")
                        .HasColumnType("uuid");

                    b.HasKey("RecipePreferenceId");

                    b.HasIndex("PreferenceId");

                    b.HasIndex("RecipeId");

                    b.ToTable("RecipePreference");
                });

            modelBuilder.Entity("AccountGroup", b =>
                {
                    b.HasOne("DOM.Accounts.Account", null)
                        .WithMany()
                        .HasForeignKey("AccountsAccountId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("DOM.Accounts.Group", null)
                        .WithMany()
                        .HasForeignKey("GroupsGroupId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });

            modelBuilder.Entity("DOM.Accounts.AccountPreference", b =>
                {
                    b.HasOne("DOM.Accounts.Account", "Account")
                        .WithMany()
                        .HasForeignKey("AccountId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("DOM.Accounts.Preference", "Preference")
                        .WithMany()
                        .HasForeignKey("PreferenceId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Account");

                    b.Navigation("Preference");
                });

            modelBuilder.Entity("DOM.Accounts.Group", b =>
                {
                    b.HasOne("DOM.MealPlanning.GroceryList", "GroceryList")
                        .WithOne("Group")
                        .HasForeignKey("DOM.Accounts.Group", "GroceryListId");

                    b.HasOne("DOM.MealPlanning.MealPlanner", "MealPlanner")
                        .WithOne("Group")
                        .HasForeignKey("DOM.Accounts.Group", "PlannerId");

                    b.Navigation("GroceryList");

                    b.Navigation("MealPlanner");
                });

            modelBuilder.Entity("DOM.Accounts.Invitation", b =>
                {
                    b.HasOne("DOM.Accounts.Group", "Group")
                        .WithMany()
                        .HasForeignKey("GroupId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("DOM.Accounts.Account", "Inviter")
                        .WithMany()
                        .HasForeignKey("InviterId")
                        .OnDelete(DeleteBehavior.Restrict)
                        .IsRequired();

                    b.Navigation("Group");

                    b.Navigation("Inviter");
                });

            modelBuilder.Entity("DOM.Accounts.Review", b =>
                {
                    b.HasOne("DOM.Accounts.Account", "Account")
                        .WithMany("Reviews")
                        .HasForeignKey("AccountId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("DOM.Recipes.Recipe", "Recipe")
                        .WithMany("Reviews")
                        .HasForeignKey("RecipeId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.Navigation("Account");

                    b.Navigation("Recipe");
                });

            modelBuilder.Entity("DOM.MealPlanning.GroceryList", b =>
                {
                    b.HasOne("DOM.Accounts.Account", "Account")
                        .WithOne("GroceryList")
                        .HasForeignKey("DOM.MealPlanning.GroceryList", "AccountId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.Navigation("Account");
                });

            modelBuilder.Entity("DOM.MealPlanning.MealPlanner", b =>
                {
                    b.HasOne("DOM.Accounts.Account", "Account")
                        .WithOne("Planner")
                        .HasForeignKey("DOM.MealPlanning.MealPlanner", "AccountId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.Navigation("Account");
                });

            modelBuilder.Entity("DOM.MealPlanning.PlannedMeal", b =>
                {
                    b.HasOne("DOM.MealPlanning.MealPlanner", "HistoryMealPlanner")
                        .WithMany("History")
                        .HasForeignKey("HistoryMealPlannerId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("DOM.MealPlanning.MealPlanner", "NextWeekMealPlanner")
                        .WithMany("NextWeek")
                        .HasForeignKey("NextWeekMealPlannerId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("DOM.Recipes.Recipe", "Recipe")
                        .WithMany("PlannedMeals")
                        .HasForeignKey("RecipeId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.Navigation("HistoryMealPlanner");

                    b.Navigation("NextWeekMealPlanner");

                    b.Navigation("Recipe");
                });

            modelBuilder.Entity("DOM.Recipes.FavoriteRecipe", b =>
                {
                    b.HasOne("DOM.Accounts.Account", "Account")
                        .WithMany("FavoriteRecipes")
                        .HasForeignKey("AccountId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("DOM.Recipes.Recipe", "Recipe")
                        .WithMany("FavoriteRecipes")
                        .HasForeignKey("RecipeId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.Navigation("Account");

                    b.Navigation("Recipe");
                });

            modelBuilder.Entity("DOM.Recipes.Ingredients.IngredientQuantity", b =>
                {
                    b.HasOne("DOM.MealPlanning.GroceryList", "GroceryList")
                        .WithMany("Ingredients")
                        .HasForeignKey("GroceryListId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("DOM.Recipes.Ingredients.Ingredient", "Ingredient")
                        .WithMany("IngredientQuantities")
                        .HasForeignKey("IngredientId");

                    b.HasOne("DOM.MealPlanning.PlannedMeal", "PlannedMeal")
                        .WithMany("Ingredients")
                        .HasForeignKey("PlannedMealId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.HasOne("DOM.Recipes.Recipe", "Recipe")
                        .WithMany("Ingredients")
                        .HasForeignKey("RecipeId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.Navigation("GroceryList");

                    b.Navigation("Ingredient");

                    b.Navigation("PlannedMeal");

                    b.Navigation("Recipe");
                });

            modelBuilder.Entity("DOM.Recipes.Ingredients.ItemQuantity", b =>
                {
                    b.HasOne("DOM.MealPlanning.GroceryItem", "GroceryItem")
                        .WithMany("ItemQuantities")
                        .HasForeignKey("GroceryItemId");

                    b.HasOne("DOM.MealPlanning.GroceryList", "GroceryList")
                        .WithMany("Items")
                        .HasForeignKey("GroceryListId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.Navigation("GroceryItem");

                    b.Navigation("GroceryList");
                });

            modelBuilder.Entity("DOM.Recipes.InstructionStep", b =>
                {
                    b.HasOne("DOM.Recipes.Recipe", "Recipe")
                        .WithMany("Instructions")
                        .HasForeignKey("RecipeId")
                        .OnDelete(DeleteBehavior.Cascade);

                    b.Navigation("Recipe");
                });

            modelBuilder.Entity("DOM.Recipes.RecipePreference", b =>
                {
                    b.HasOne("DOM.Accounts.Preference", "Preference")
                        .WithMany()
                        .HasForeignKey("PreferenceId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.HasOne("DOM.Recipes.Recipe", "Recipe")
                        .WithMany()
                        .HasForeignKey("RecipeId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("Preference");

                    b.Navigation("Recipe");
                });

            modelBuilder.Entity("DOM.Accounts.Account", b =>
                {
                    b.Navigation("FavoriteRecipes");

                    b.Navigation("GroceryList");

                    b.Navigation("Planner");

                    b.Navigation("Reviews");
                });

            modelBuilder.Entity("DOM.MealPlanning.GroceryItem", b =>
                {
                    b.Navigation("ItemQuantities");
                });

            modelBuilder.Entity("DOM.MealPlanning.GroceryList", b =>
                {
                    b.Navigation("Group");

                    b.Navigation("Ingredients");

                    b.Navigation("Items");
                });

            modelBuilder.Entity("DOM.MealPlanning.MealPlanner", b =>
                {
                    b.Navigation("Group");

                    b.Navigation("History");

                    b.Navigation("NextWeek");
                });

            modelBuilder.Entity("DOM.MealPlanning.PlannedMeal", b =>
                {
                    b.Navigation("Ingredients");
                });

            modelBuilder.Entity("DOM.Recipes.Ingredients.Ingredient", b =>
                {
                    b.Navigation("IngredientQuantities");
                });

            modelBuilder.Entity("DOM.Recipes.Recipe", b =>
                {
                    b.Navigation("FavoriteRecipes");

                    b.Navigation("Ingredients");

                    b.Navigation("Instructions");

                    b.Navigation("PlannedMeals");

                    b.Navigation("Reviews");
                });
#pragma warning restore 612, 618
        }
    }
}
