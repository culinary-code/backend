using BL.AutoMapper;
using BL.Managers.Recipes;
using BL.ExternalSources.Llm;
using BL.Managers.Accounts;
using BL.Managers.MealPlanning;
using BL.Managers.Groceries;
using BL.Scheduled;
using BL.Services;
using Configuration.Options;
using DAL.Accounts;
using DAL.EF;
using DAL.Groceries;
using DAL.MealPlanning;
using DAL.Recipes;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Quartz;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);
builder.Configuration.AddEnvironmentVariables();

// load environment variables
IConfiguration configuration = builder.Configuration;

builder.Services.AddOptions<AzureOpenAIOptions>()
    .Bind(configuration.GetSection("AzureOpenAI"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddOptions<AzureStorageOptions>()
    .Bind(configuration.GetSection("AzureStorage"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddOptions<KeycloakOptions>()
    .Bind(configuration.GetSection("Keycloak"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddOptions<JobSettingsOptions>()
    .Bind(configuration.GetSection("RecipeJob"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddOptions<LocalLlmServerOptions>()
    .Bind(configuration.GetSection("LocalLLMServer"))
    .ValidateDataAnnotations()
    .ValidateOnStart();

builder.Services.AddOptions<DatabaseOptions>()
    .Bind(configuration.GetSection("Database"))
    .ValidateDataAnnotations()
    .ValidateOnStart();


var databaseOptions = builder.Configuration.GetSection("Database").Get<DatabaseOptions>();
var connectionString = databaseOptions!.ConnectionString;
builder.Services.AddDbContext<CulinaryCodeDbContext>(optionsBuilder =>
    optionsBuilder.UseNpgsql(connectionString));

// Add a named HttpClient to be used for Keycloak
builder.Services.AddHttpClient("Keycloak", httpClient =>
    {
        var keycloakOptions = builder.Configuration.GetSection("Keycloak").Get<KeycloakOptions>();
        var baseUrl = keycloakOptions!.BaseUrl;
        httpClient.BaseAddress = new Uri(baseUrl);
    }
);

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
builder.Services.AddSingleton<ILlmService, AzureOpenAIService>();

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

var keycloakOptions = builder.Configuration.GetSection("Keycloak").Get<KeycloakOptions>();
var baseUrl = keycloakOptions!.BaseUrl;
var frontendUrl = keycloakOptions!.FrontendUrl;
var clientId = keycloakOptions!.ClientId;
var realm = keycloakOptions!.Realm;

var authority = baseUrl + "/realms/" + realm;
var issuer = frontendUrl + "/realms/" + realm;

var isDevelopment = builder.Environment.IsDevelopment();

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = issuer;
        options.Audience = "account";

        // For development, allow HTTP
        options.RequireHttpsMetadata = !isDevelopment;

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
                var httpClient = builder.Services.BuildServiceProvider().GetRequiredService<IHttpClientFactory>().CreateClient("Keycloak");
                var jwks = httpClient.GetStringAsync($"/realms/{realm}/protocol/openid-connect/certs").Result;
                var keys = new JsonWebKeySet(jwks);
                return keys.GetSigningKeys();
            }
        };
    });

// Scheduled jobs
var jobSettingsOptions = builder.Configuration.GetSection("RecipeJob").Get<JobSettingsOptions>();
var cronSchedule = jobSettingsOptions!.CronSchedule;

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
    
    // Not needed for production, as the frontend will be a mobile application, which does not handle CORS
    // when developing locally and using the device preview on web, allow origins on localhost
    // swap out to AllowAllOrigins if your frontend is not running on localhost 
    app.UseCors("localWebOrigin");
}
else
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();