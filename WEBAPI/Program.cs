using BL.AutoMapper;
using BL.Managers.Recipes;
using BL.ExternalSources.Llm;
using BL.Managers.Accounts;
using BL.Managers.MealPlanning;
using BL.Managers.Groceries;
using BL.Scheduled;
using BL.Services;
using DAL.Accounts;
using DAL.EF;
using DAL.Groceries;
using DAL.MealPlanning;
using DAL.Recipes;
using DOM.Exceptions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Quartz;
using Quartz.Impl;
using Quartz.Simpl;
using Quartz.Spi;

// load environment variables
DotNetEnv.Env.Load("../.env");

var builder = WebApplication.CreateBuilder(args);

// set DbContext
//builder.Services.AddDbContext<CulinaryCodeDbContext>(optionsBuilder => 
//    optionsBuilder.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

var connectionString = Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING") ?? throw new EnvironmentVariableNotAvailableException("DATABASE_CONNECTION_STRING environment variable is not set.");
Console.WriteLine("Connection string: " + connectionString);
builder.Services.AddDbContext<CulinaryCodeDbContext>(optionsBuilder => 
    optionsBuilder.UseNpgsql(connectionString));



// Add services to the container.

// Repositories
builder.Services.AddScoped<IRecipeRepository, RecipeRepository>();
builder.Services.AddScoped<IIngredientRepository, IngredientRepository>();
builder.Services.AddScoped<IPreferenceRepository, PreferenceRepository>();
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IMealPlannerRepository, MealPlannerRepository>();
builder.Services.AddScoped<IGroceryRepository, GroceryRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();
builder.Services.AddScoped<IPreferenceRepository, PreferenceRepository>();
builder.Services.AddScoped<IGroupRepository, GroupRepository>();
builder.Services.AddScoped<IInvitationRepository, InvitationRepository>();


// Managers
builder.Services.AddScoped<IRecipeManager, RecipeManager>();
builder.Services.AddScoped<IAccountManager, AccountManager>();
builder.Services.AddScoped<IMealPlannerManager, MealPlannerManager>();
builder.Services.AddScoped<IGroceryManager, GroceryManager>();
builder.Services.AddScoped<IReviewManager, ReviewManager>();
builder.Services.AddScoped<IPreferenceManager, PreferenceManager>();
builder.Services.AddScoped<IGroupManager, GroupManager>();
builder.Services.AddScoped<IInvitationManager, InvitationManager>();

// Services
builder.Services.AddHttpClient<IIdentityProviderService, KeyCloakService>();
builder.Services.AddScoped<IIdentityProviderService, KeyCloakService>();
builder.Services.AddScoped<IEmailService, EmailService>();

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


// Authorization / Authentication
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAllOrigins",
        corsBuilder =>
        {
            corsBuilder.AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader();
        });
    options.AddPolicy("localWebOrigin",
        corsBuilder =>
    {
        corsBuilder.SetIsOriginAllowed(origin => new Uri(origin).Host == "localhost")
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var baseUrl = Environment.GetEnvironmentVariable("KEYCLOAK_BASE_URL") ?? throw new EnvironmentVariableNotAvailableException("KEYCLOAK_BASE_URL environment variable is not set.");
var frontendUrl = Environment.GetEnvironmentVariable("KEYCLOAK_FRONTEND_URL") ?? throw new EnvironmentVariableNotAvailableException("KEYCLOAK_FRONTEND_URL environment variable is not set.");
var clientId = Environment.GetEnvironmentVariable("KEYCLOAK_CLIENT_ID") ?? throw new EnvironmentVariableNotAvailableException("KEYCLOAK_CLIENT_ID environment variable is not set.");
var realm = Environment.GetEnvironmentVariable("KEYCLOAK_REALM") ?? throw new EnvironmentVariableNotAvailableException("KEYCLOAK_REALM environment variable is not set.");

var authority = baseUrl + "/realms/" + realm;
var issuer = frontendUrl + "/realms/" + realm;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = issuer;
        options.Audience = "account";
        
        options.RequireHttpsMetadata = false;
        
        // Optionally configure token validation parameters
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = issuer, // The issuer URL Keycloak is using

            ValidateAudience = true,
            ValidAudience = "account", // Must match the client ID in Keycloak

            ValidateLifetime = true,
            // ClockSkew = TimeSpan.Zero, // Optional, remove any clock skew tolerance

            ValidateIssuerSigningKey = true,
            // Optionally, you can validate the signing key using Keycloak's JWKS endpoint
            IssuerSigningKeyResolver = (token, securityToken, kid, parameters) =>
            {
                // Keycloak's JWKS endpoint to retrieve public signing keys
                var httpClient = new HttpClient();
                var jwks = httpClient.GetStringAsync($"{authority}/protocol/openid-connect/certs").Result;
                var keys = new JsonWebKeySet(jwks);
                return keys.GetSigningKeys();
            }
        };
    });

// Scheduled jobs

var cronSchedule = Environment.GetEnvironmentVariable("RECIPE_JOB_CRON_SCHEDULE") ?? throw new EnvironmentVariableNotAvailableException("RECIPE_JOB_CRON_SCHEDULE environment variable is not set.");

builder.Services.AddQuartz(q =>
{
    // Register the job and trigger
    q.AddJob<RefreshRecipeDatabaseJob>(opts => opts.WithIdentity("RefreshRecipeDatabaseJob"));
    q.AddTrigger(opts => opts
        .ForJob("RefreshRecipeDatabaseJob") // Link to the registered job
        .WithIdentity("RefreshRecipeDatabaseJob-trigger") // Name of the trigger
        .WithCronSchedule(cronSchedule)); // CRON trigger at 2am
});

// Add the Quartz Hosted Service
builder.Services.AddQuartzHostedService(options =>
{
    options.WaitForJobsToComplete = true; // Optional: Wait for jobs to complete before shutting down
});



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
    // app.UseCors("localWebOrigin");
    app.UseCors("AllowAllOrigins");
    // TODO: when deploying to a real backend instead of a docker container, check if it works with mobile.
}
else
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();