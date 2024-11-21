using BL.AutoMapper;
using BL.Managers.Recipes;
using BL.ExternalSources.Llm;
using BL.Managers.Accounts;
using BL.Services;
using DAL.Accounts;
using DAL.EF;
using DAL.Recipes;
using DOM.Exceptions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;

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
builder.Services.AddScoped<IAccountRepository, AccountRepository>();
builder.Services.AddScoped<IReviewRepository, ReviewRepository>();


// Managers
builder.Services.AddScoped<IRecipeManager, RecipeManager>();
builder.Services.AddScoped<IAccountManager, AccountManager>();
builder.Services.AddScoped<IReviewManager, ReviewManager>();

// Services
builder.Services.AddHttpClient<IIdentityProviderService, KeyCloakService>();
builder.Services.AddScoped<IIdentityProviderService, KeyCloakService>();

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
var clientId = Environment.GetEnvironmentVariable("KEYCLOAK_CLIENT_ID") ?? throw new EnvironmentVariableNotAvailableException("KEYCLOAK_CLIENT_ID environment variable is not set.");
var realm = Environment.GetEnvironmentVariable("KEYCLOAK_REALM") ?? throw new EnvironmentVariableNotAvailableException("KEYCLOAK_REALM environment variable is not set.");

var authority = baseUrl + "/realms/" + realm;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = authority;
        options.Audience = "account";
        
        options.RequireHttpsMetadata = false;
        
        // Optionally configure token validation parameters
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = authority, // The issuer URL Keycloak is using

            ValidateAudience = true,
            ValidAudience = "account", // Must match the client ID in Keycloak

            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero, // Optional, remove any clock skew tolerance

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
    app.UseCors("localWebOrigin");
    // app.UseCors("AllowAllOrigins");
    // TODO: when deploying to a real backend instead of a docker container, check if it works with mobile.
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();