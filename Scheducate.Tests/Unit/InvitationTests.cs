using Scheducate.BusinessRules;
using Xunit;

namespace Scheducate.Tests.Unit;

public class InvitationTests
{
    [Fact]
    public void IsExpired_InvitationOlderThanSevenDays_ReturnsTrue()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var createdAt = now.AddDays(-8);

        // Act
        var result = InvitationRules.IsExpired(createdAt, now);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public void IsExpired_InvitationYoungerThanSevenDays_ReturnsFalse()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var createdAt = now.AddDays(-3);

        // Act
        var result = InvitationRules.IsExpired(createdAt, now);

        // Assert
        Assert.False(result);
    }

    [Fact]
    public void IsExpired_InvitationExactlySevenDaysOld_ReturnsTrue()
    {
        // Arrange
        var now = DateTime.UtcNow;
        var createdAt = now.AddDays(-7);

        // Act
        var result = InvitationRules.IsExpired(createdAt, now);

        // Assert
        Assert.True(result);
    }
}