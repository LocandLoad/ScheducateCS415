namespace Scheducate.BusinessRules;

public static class ScheduleRules
{
    public static bool HasValidAvailability(byte[]? availability)
    {
        return availability != null && availability.Length == 42;
    }
}