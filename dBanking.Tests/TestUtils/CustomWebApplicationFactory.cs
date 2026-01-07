
using dBanking.Infrastructure.DbContext;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using System;
using System.Data.Common;

namespace dBanking.Tests.TestUtils
{
    public sealed class CustomWebApplicationFactory : WebApplicationFactory<Program>
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // Replace AppPostgresDbContext with SQLite In-Memory
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<AppPostgresDbContext>));
                if (descriptor != null) services.Remove(descriptor);

                var connection = new SqliteConnection("DataSource=:memory:");
                connection.Open();

                services.AddDbContext<AppPostgresDbContext>(options =>
                {
                    options.UseSqlite(connection);
                });

                // Apply migrations/ensure database
                using var scope = services.BuildServiceProvider().CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppPostgresDbContext>();
                db.Database.EnsureCreated();

                // Optionally: replace MassTransit with TestHarness or disable bus for controller tests.
            });
        }
    }
}
