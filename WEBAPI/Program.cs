using BL.AutoMapper;
using BL.Managers.Recipes;
using BL.ExternalSources.Llm;
using DAL.EF;
using DAL.Recipes;
using Microsoft.EntityFrameworkCore;

// load environment variables
DotNetEnv.Env.Load("../.env");

var builder = WebApplication.CreateBuilder(args);

// set DbContext
builder.Services.AddDbContext<CulinaryCodeDbContext>(optionsBuilder => 
    optionsBuilder.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));


// Add services to the container.

// Repositories
builder.Services.AddScoped<IRecipeRepository, RecipeRepository>();
builder.Services.AddScoped<IIngredientRepository, IngredientRepository>();
builder.Services.AddScoped<IPreferenceRepository, PreferenceRepository>();


// Managers
builder.Services.AddScoped<IRecipeManager, RecipeManager>();

// Automapper
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Controllers
builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Logging
builder.Services.AddLogging();

// External services
builder.Services.AddSingleton<LlmSettingsService>();

// swap comment to switch between local and azure service
builder.Services.AddSingleton<ILlmService, AzureOpenAIService>(); 
//builder.Services.AddSingleton<ILlmService, LocalLlmService>();


var app = builder.Build();

// TODO: remove the fragment below
// currently in here to make sure the database works.
// Since there is nothing calling upon the scoped dbContext at startup it would not function otherwise.
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<CulinaryCodeDbContext>();
    dbContext.Database.EnsureCreated(); // Use only if you don't want to use migrations
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();