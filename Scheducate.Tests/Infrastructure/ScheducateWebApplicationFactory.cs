//Configures the application for testing, including an in-memory SQLite database and test authentication.
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Scheducate.Data;

namespace Scheducate.Tests.Infrastructure;

public class ScheducateWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection =
        new("Data Source=:memory:");

    public ScheducateWebApplicationFactory()
    {
        _connection.Open();
    }

    

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<ApplicationDbContext>();
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlite(_connection));

            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = TestAuthHandler.SchemeName;
                options.DefaultChallengeScheme = TestAuthHandler.SchemeName;
                options.DefaultScheme = TestAuthHandler.SchemeName;
            })

            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                TestAuthHandler.SchemeName,
                options => { });
        });

        builder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
            logging.SetMinimumLevel(LogLevel.None);
        });

        builder.ConfigureServices(services =>
        {
            services.AddLogging(logging =>
            {
                logging.SetMinimumLevel(LogLevel.None);
                logging.AddFilter("Microsoft.EntityFrameworkCore", LogLevel.None);
                logging.AddFilter("Microsoft.AspNetCore", LogLevel.None);
            });
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);

        using var scope = host.Services.CreateScope();

        var context = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        context.Database.EnsureCreated();

        return host;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            _connection.Dispose();
        }
    }
}