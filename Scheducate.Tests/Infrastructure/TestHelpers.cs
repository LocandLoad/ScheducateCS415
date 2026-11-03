// Provides helper functions for creating users and retriving antiforgery tokens
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Scheducate.Data;
using Scheducate.Models;
using System.Net;
using System.Text.RegularExpressions;

namespace Scheducate.Tests.Infrastructure;

public static class TestHelpers
{
    public static async Task SeedUserAsync(
        ScheducateWebApplicationFactory factory,
        string userId)
    {
        using var scope = factory.Services.CreateScope();

        var context = scope.ServiceProvider
            .GetRequiredService<ApplicationDbContext>();

        if (!await context.Users.AnyAsync(u => u.Id == userId))
        {
            context.Users.Add(new User
            {
                Id = userId,
                UserName = $"{userId}@example.com",
                NormalizedUserName = $"{userId.ToUpperInvariant()}@EXAMPLE.COM",
                Email = $"{userId}@example.com",
                NormalizedEmail = $"{userId.ToUpperInvariant()}@EXAMPLE.COM",
                EmailConfirmed = true
            });

            await context.SaveChangesAsync();
        }
    }

    public static async Task<string> GetAntiforgeryTokenAsync(
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
            throw new Exception("Antiforgery token was not found.");

        var valueMatch = Regex.Match(
            inputMatch.Value,
            @"\bvalue\s*=\s*[""']([^""']+)[""']",
            RegexOptions.IgnoreCase);

        if (!valueMatch.Success)
            throw new Exception("Antiforgery token value was not found.");

        return WebUtility.HtmlDecode(valueMatch.Groups[1].Value);
    }
}