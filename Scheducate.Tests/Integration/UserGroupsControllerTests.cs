//Tests for user-group invitation functionality.
using System.Net;
using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Scheducate.Data;
using Scheducate.Models;
using Scheducate.Tests.Infrastructure;
using Xunit;

namespace Scheducate.Tests.Integration;

public class UserGroupsControllerTests
{
    private const string TestUserId = "test-user";

   
    // TEST 1: A valid invitation can be accepted
    [Fact]
    public async Task AcceptInvitation_ValidInvitation_AddsMember()
    {
        using var factory = new ScheducateWebApplicationFactory();

        var client = factory.CreateClient(
            new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

        var (groupId, token) = await SeedInvitationAsync(
            factory,
            DateTime.UtcNow.AddDays(-1));

        // Retrieve antiforgery token from the invitation page
        var antiforgeryToken = await GetAntiforgeryTokenAsync(
            client,
            $"/UserGroups/Join/{token}");

        // Submit invitation acceptance
        var response = await client.PostAsync(
            "/UserGroups/AcceptInvitation",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["token"] = token,
                ["__RequestVerificationToken"] = antiforgeryToken
            }));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);

        // Verify membership was created and invitation removed
        using var scope = factory.Services.CreateScope();

        var context = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        var membershipExists = await context.GroupMembers.AnyAsync(
            m => m.GroupId == groupId && m.UserId == TestUserId);

        var invitationExists = await context.GroupInvitations.AnyAsync(
            i => i.Token == token);

        Assert.True(membershipExists);
        Assert.False(invitationExists);
    }



    // TEST 2: Expired invitations are rejected
    [Fact]
    public async Task AcceptInvitation_ExpiredInvitation_ReturnsBadRequest()
    {
        using var factory = new ScheducateWebApplicationFactory();

        var client = factory.CreateClient(
            new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

        var (groupId, token) = await SeedInvitationAsync(
            factory,
            DateTime.UtcNow.AddDays(-8));

        var antiforgeryToken = await GetAntiforgeryTokenAsync(
            client,
            $"/UserGroups/Join/{token}");

        var response = await client.PostAsync(
            "/UserGroups/AcceptInvitation",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["token"] = token,
                ["__RequestVerificationToken"] = antiforgeryToken
            }));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        // Expired invitations should not create memberships
        using var scope = factory.Services.CreateScope();

        var context = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        var membershipExists = await context.GroupMembers.AnyAsync(
            m => m.GroupId == groupId && m.UserId == TestUserId);

        var invitationExists = await context.GroupInvitations.AnyAsync(
            i => i.Token == token);

        Assert.False(membershipExists);
        Assert.True(invitationExists);
    }


    // TEST 3: POST requests without antiforgery tokens fail
    [Fact]
    public async Task AcceptInvitation_MissingAntiforgeryToken_ReturnsBadRequest()
    {
        using var factory = new ScheducateWebApplicationFactory();

        var client = factory.CreateClient(
            new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

        await SeedInvitationAsync(
            factory,
            DateTime.UtcNow.AddDays(-1));

        // Deliberately submit without an antiforgery token
        var response = await client.PostAsync(
            "/UserGroups/AcceptInvitation",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["token"] = "some-token"
            }));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }



    // TEST 4: Invitations cannot be used twice
    [Fact]
    public async Task AcceptInvitation_AlreadyUsedInvitation_ReturnsNotFound()
    {
        using var factory = new ScheducateWebApplicationFactory();

        var client = factory.CreateClient(
            new Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactoryClientOptions
            {
                AllowAutoRedirect = false
            });

        var (_, token) = await SeedInvitationAsync(
            factory,
            DateTime.UtcNow.AddDays(-1));

        var antiforgeryToken = await GetAntiforgeryTokenAsync(
            client,
            $"/UserGroups/Join/{token}");

        // First acceptance
        var firstResponse = await client.PostAsync(
            "/UserGroups/AcceptInvitation",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["token"] = token,
                ["__RequestVerificationToken"] = antiforgeryToken
            }));

        Assert.Equal(HttpStatusCode.Redirect, firstResponse.StatusCode);

        // Second acceptance using the same invitation
        var secondResponse = await client.PostAsync(
            "/UserGroups/AcceptInvitation",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["token"] = token,
                ["__RequestVerificationToken"] = antiforgeryToken
            }));

        Assert.Equal(HttpStatusCode.NotFound, secondResponse.StatusCode);
    }


    // TEST 5: Invalid tokens are rejected
    [Fact]
    public async Task AcceptInvitation_InvalidAntiforgeryToken_ReturnsBadRequest()
    {
        using var factory = new ScheducateWebApplicationFactory();
        var client = factory.CreateClient();

        var response = await client.PostAsync(
            "/UserGroups/AcceptInvitation",
            new FormUrlEncodedContent(new Dictionary<string, string>
            {
                ["token"] = "some-token",
                ["__RequestVerificationToken"] = "invalid-token"
            }));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    // HELPER: Create a test group and invitation


    private static async Task<(int GroupId, string Token)>
        SeedInvitationAsync(
            ScheducateWebApplicationFactory factory,
            DateTime createdAt)
    {
        using var scope = factory.Services.CreateScope();

        var context = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        // Create authenticated test user
        var user = await context.Users.FindAsync(TestUserId);

        if (user == null)
        {
            user = new User
            {
                Id = TestUserId,
                UserName = "test@example.com",
                NormalizedUserName = "TEST@EXAMPLE.COM",
                Email = "test@example.com",
                NormalizedEmail = "TEST@EXAMPLE.COM",
                EmailConfirmed = true
            };

            context.Users.Add(user);
        }
                // Create separate group owner
        var owner = await context.Users.FindAsync("test-owner");

        if (owner == null)
        {
            owner = new User
            {
                Id = "test-owner",
                UserName = "owner@example.com",
                NormalizedUserName = "OWNER@EXAMPLE.COM",
                Email = "owner@example.com",
                NormalizedEmail = "OWNER@EXAMPLE.COM",
                EmailConfirmed = true
            };

            context.Users.Add(owner);
        }

        // Create test group
        var group = new UserGroup
        {
            Name = "Test Group",
            OwnerId = "test-owner"
        };

        context.UserGroups.Add(group);

        await context.SaveChangesAsync();

        // Create invitation
        var token = Guid.NewGuid().ToString("N");

        var invitation = new GroupInvitation
        {
            GroupId = group.Id,
            Token = token,
            CreatedAt = createdAt
        };

        context.GroupInvitations.Add(invitation);

        await context.SaveChangesAsync();

        return (group.Id, token);
    }



    // HELPER: Retrieve antiforgery token from a GET page

    private static async Task<string> GetAntiforgeryTokenAsync(
        HttpClient client,
        string pageUrl)
    {
        var response = await client.GetAsync(pageUrl);

        response.EnsureSuccessStatusCode();

        var html = await response.Content.ReadAsStringAsync();

        var inputMatch = Regex.Match(
            html,
            @"<input\b(?=[^>]*\bname\s*=\s*[""']__RequestVerificationToken[""'])[^>]*>",
            RegexOptions.IgnoreCase);

        if (!inputMatch.Success)
        {
            throw new Exception(
                "Antiforgery token was not found in the HTML form.");
        }

        var valueMatch = Regex.Match(
            inputMatch.Value,
            @"\bvalue\s*=\s*[""']([^""']+)[""']",
            RegexOptions.IgnoreCase);

        if (!valueMatch.Success)
        {
            throw new Exception(
                "Antiforgery token value was not found.");
        }

        return WebUtility.HtmlDecode(valueMatch.Groups[1].Value);
    }
}