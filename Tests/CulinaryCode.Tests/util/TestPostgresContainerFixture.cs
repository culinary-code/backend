using DAL.EF;
using Microsoft.EntityFrameworkCore;
using Testcontainers.PostgreSql;

namespace CulinaryCode.Tests.util;

public class TestPostgresContainerFixture : IAsyncLifetime
{
    protected readonly PostgreSqlContainer _postgresContainer;
    public CulinaryCodeDbContext DbContext { get; private set; }
    
    public TestPostgresContainerFixture()
    {
        // Initialize the PostgreSQL Testcontainer
        _postgresContainer = new PostgreSqlBuilder()
            .WithDatabase("culinarycode_test")
            .WithUsername("testuser")
            .WithPassword("testpassword")
            .Build();
    }
    
    public async Task InitializeAsync()
    {
        await _postgresContainer.StartAsync();

        var options = new DbContextOptionsBuilder<CulinaryCodeDbContext>()
            .UseNpgsql(_postgresContainer.GetConnectionString())
            .Options;

        DbContext = new CulinaryCodeDbContext(options);
        await DbContext.Database.EnsureCreatedAsync();
    }
    
    // Cleanup between tests
    public async Task ResetDatabaseAsync()
    {
        await DbContext.Database.EnsureDeletedAsync();
        await DbContext.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await DbContext.DisposeAsync();
        await _postgresContainer.StopAsync();
    }
}