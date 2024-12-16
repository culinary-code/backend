using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using System.IO;

namespace DAL.EF;

public class CulinaryCodeDbContextFactory : IDesignTimeDbContextFactory<CulinaryCodeDbContext>
{
    public CulinaryCodeDbContext CreateDbContext(string[] args)
    {
        // Load configuration settings (such as connection string) from appsettings.json
        var optionsBuilder = new DbContextOptionsBuilder<CulinaryCodeDbContext>();

        // Get the current directory
        var basePath = Directory.GetCurrentDirectory();
            
        // Load configuration from the WEBAPI project
        var configuration = new ConfigurationBuilder()
            .SetBasePath(basePath + "/../WEBAPI")  // Point to the WEBAPI project directory
            .AddJsonFile("appsettings.json")  // Load the appsettings.json
            .Build();

        // Check if an environment variable for the connection string exists
        var connectionString = Environment.GetEnvironmentVariable("Database__ConnectionString");

        if (string.IsNullOrEmpty(connectionString))
        {
            // If the environment variable is not set, fall back to the connection string in appsettings.json
            connectionString = configuration.GetConnectionString("DefaultConnection");
        }

        // Configure the DbContext to use Npgsql (PostgreSQL)
        optionsBuilder.UseNpgsql(connectionString);

        return new CulinaryCodeDbContext(optionsBuilder.Options);
    }
}