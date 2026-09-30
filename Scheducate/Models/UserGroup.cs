namespace Scheducate.Models
{
    public class UserGroup
    {
        public int Id { get; set; }

        public required string Name { get; set; }

        // Owner
        public string OwnerId { get; set; } = null!;
        public User? Owner { get; set; }

        // Members
        public ICollection<GroupMember> Members { get; set; } = [];

        // Invitations
        public ICollection<GroupInvitation> Invitations { get; set; } = [];
    }
}
