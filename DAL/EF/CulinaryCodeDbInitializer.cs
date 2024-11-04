using Microsoft.EntityFrameworkCore;

namespace DAL.EF;

internal static class CulinaryCodeDbInitializer
{
    public static void Initialize(CulinaryCodeDbContext context, bool dropCreateDatabase = false)
    {
        if (dropCreateDatabase)
            context.Database.EnsureDeleted();

        if (context.Database.EnsureCreated())
            Seed(context);
    }

    private static void Seed(CulinaryCodeDbContext context)
    {
        // Add objects here
        
        // Save changes
        context.SaveChanges();
        
        // Clear change-tracker for the data does not stay tracked all the time
        context.ChangeTracker.Clear();
    }
}