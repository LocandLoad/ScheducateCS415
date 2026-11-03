using Microsoft.Extensions.DependencyInjection;
using Scheducate.Data;
using Scheducate.Models;
using Scheducate.Tests.Infrastructure;

namespace Scheducate.Tests.Integration;

public class SchedulesControllerTests
{
    [Fact]
    public async Task Index_OnlyDisplaysSchedulesBelongingToCurrentUser()
    {
        // Arrange: Start the test application.
        using var factory = new ScheducateWebApplicationFactory();
        var client = factory.CreateClient();

        // Add two users and their schedules to the test database.
        using (var scope = factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();

            context.Users.AddRange(
                new User
                {
                    Id = "test-user",
                    UserName = "test@example.com",
                    Email = "test@example.com"
                },
                new User
                {
                    Id = "other-user",
                    UserName = "other@example.com",
                    Email = "other@example.com"
                });

            context.Schedule.AddRange(
                new Schedule
                {
                    Name = "My Schedule",
                    UserId = "test-user"
                },
                new Schedule
                {
                    Name = "Other User Schedule",
                    UserId = "other-user"
                });

            await context.SaveChangesAsync();
        }

        // Act: Request the schedules page as the authenticated test user.
        var response = await client.GetAsync("/Schedules");
        var html = await response.Content.ReadAsStringAsync();

        // Assert: The page only displays the current user's schedule.
        response.EnsureSuccessStatusCode();

        Assert.Contains("My Schedule", html);
        Assert.DoesNotContain("Other User Schedule", html);
    }
}