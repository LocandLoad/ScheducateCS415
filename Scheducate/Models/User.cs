using Microsoft.AspNetCore.Identity;
using Scheducate.Models;

public class User : IdentityUser
{
    public ICollection<Schedule> Schedules { get; set; } = [];

    public ICollection<GroupMember> GroupMemberships { get; set; } = [];

    public ICollection<UserGroup> OwnedGroups { get; set; } = [];
}
