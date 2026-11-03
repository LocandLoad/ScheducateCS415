// Tests schedule availability validation
using Scheducate.BusinessRules;

namespace Scheducate.Tests.Unit;

public class ScheduleTests
{
    [Fact]
    public void HasValidAvailability_Exactly42Bytes_ReturnsTrue()
    {
        var availability = new byte[42];

        Assert.True(
            ScheduleRules.HasValidAvailability(availability));
    }

    [Fact]
    public void HasValidAvailability_LessThan42Bytes_ReturnsFalse()
    {
        var availability = new byte[20];

        Assert.False(
            ScheduleRules.HasValidAvailability(availability));
    }

    [Fact]
    public void HasValidAvailability_MoreThan42Bytes_ReturnsFalse()
    {
        var availability = new byte[50];

        Assert.False(
            ScheduleRules.HasValidAvailability(availability));
    }

    [Fact]
    public void HasValidAvailability_Null_ReturnsFalse()
    {
        Assert.False(
            ScheduleRules.HasValidAvailability(null));
    }
    [Fact]
    public void HasValidAvailability_EmptyArray_ReturnsFalse()
    {
        var availability = Array.Empty<byte>();

        Assert.False(
            ScheduleRules.HasValidAvailability(availability));
    }

    [Fact]
    public void HasValidAvailability_Exactly41Bytes_ReturnsFalse()
    {
        var availability = new byte[41];

        Assert.False(
            ScheduleRules.HasValidAvailability(availability));
    }
}