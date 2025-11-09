using Microsoft.EntityFrameworkCore;
using TwoPly.Data;

namespace TwoPly.Tests.Helpers;

public static class DbContextHelper
{
    public static ApplicationDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }
}