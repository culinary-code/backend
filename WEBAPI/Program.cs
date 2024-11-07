using BL.AutoMapper;
using BL.Managers.Recipes;
using BL.ExternalSources.Llm;
using BL.Services;
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


// Managers
builder.Services.AddScoped<IRecipeManager, RecipeManager>();

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

var baseUrl = Environment.GetEnvironmentVariable("KEYCLOAK_BASE_URL") ?? throw new EnvironmentException("KEYCLOAK_BASE_URL environment variable is not set.");
var clientId = Environment.GetEnvironmentVariable("KEYCLOAK_CLIENT_ID") ?? throw new EnvironmentException("KEYCLOAK_CLIENT_ID environment variable is not set.");
var realm = Environment.GetEnvironmentVariable("KEYCLOAK_REALM") ?? throw new EnvironmentException("KEYCLOAK_REALM environment variable is not set.");

var authority = baseUrl + "/auth/realms/" + realm;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = authority;
        options.Audience = clientId;
        
        options.RequireHttpsMetadata = false;
        
        // Optionally configure token validation parameters
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true
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

app.UseAuthorization();
app.UseAuthentication();

app.MapControllers();

app.Run();