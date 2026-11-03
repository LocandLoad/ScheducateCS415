namespace Scheducate.BusinessRules;

public static class InvitationRules
{
    public static bool IsExpired(DateTime createdAt, DateTime utcNow)
    {
        return createdAt.AddDays(7) <= utcNow;
    }
}