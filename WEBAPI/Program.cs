using DAL;
using DAL.EF;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using BL.ExternalSources.ChatGPT;

var builder = WebApplication.CreateBuilder(args);

// set DbContext
builder.Services.AddDbContext<CulinaryCodeDbContext>(optionsBuilder => 
    optionsBuilder.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));


// Add services to the container.

// Repositories
builder.Services.AddScoped<IRecipeRepository, RecipeRepository>();


// Managers



builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddSingleton<AzureOpenAIService>();


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