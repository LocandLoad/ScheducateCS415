//Tests schedule connected HTTP actions, like listing schedules, 
// creating schedules, deleting schedules, and preventing users from accessing or deleting other's schedules.
using System.Net;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Scheducate.Data;
using Scheducate.Models;
using Scheducate.Tests.Infrastructure;

namespace Scheducate.Tests.Integration;

public class SchedulesControllerTests
{

    // Test 1: Only display current User's schedule.

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

    // Test 2: Delete with no ID
    [Fact]
    public async Task Delete_NullId_ReturnsNotFound()
    {
        using var factory = new ScheducateWebApplicationFactory();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/Schedules/Delete");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    //Test 3: Delete a schedule that doesn't exist
    [Fact]
    public async Task Delete_NonexistentSchedule_ReturnsNotFound()
    {
        using var factory = new ScheducateWebApplicationFactory();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/Schedules/Delete/9999");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    //Test 4: Delete another user's schedule through GET
    [Fact]
    public async Task Delete_Get_OtherUsersSchedule_ReturnsNotFound()
    {
        using var factory = new ScheducateWebApplicationFactory();
        var client = factory.CreateClient();

        await TestHelpers.SeedUserAsync(factory, "other-user");

        int scheduleId;

        using (var scope = factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();

            var schedule = new Schedule
            {
                Name = "Other User Schedule",
                UserId = "other-user",
                Availability = new byte[42]
            };

            context.Schedule.Add(schedule);
            await context.SaveChangesAsync();

            scheduleId = schedule.Id;
        }

        var response = await client.GetAsync(
            $"/Schedules/Delete/{scheduleId}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    // Test 5: Delete your own schedule through GET
    [Fact]
    public async Task Delete_Get_OwnedSchedule_ReturnsView()
    {
        using var factory = new ScheducateWebApplicationFactory();
        var client = factory.CreateClient();

        await TestHelpers.SeedUserAsync(factory, "test-user");

        int scheduleId;

        using (var scope = factory.Services.CreateScope())
        {
            var context = scope.ServiceProvider
                .GetRequiredService<ApplicationDbContext>();

            var schedule = new Schedule
            {
                Name = "My Schedule",
                UserId = "test-user",
                Availability = new byte[42]
            };

            context.Schedule.Add(schedule);
            await context.SaveChangesAsync();

            scheduleId = schedule.Id;
        }

        var response = await client.GetAsync(
            $"/Schedules/Delete/{scheduleId}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    // Test 6: Create page loads successfully and returns a new Schedule model
    [Fact]
    public async Task Create_Get_ReturnsView()
    {
        using var factory = new ScheducateWebApplicationFactory();
        var client = factory.CreateClient();

        var response = await client.GetAsync("/Schedules/Create");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}