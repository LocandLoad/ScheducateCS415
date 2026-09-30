using System.ComponentModel.DataAnnotations.Schema;

namespace Scheducate.Models
{
    public class GroupMember
    {
        public int Id { get; set; }

        // User
        public string UserId { get; set; } = null!;
        public User? User { get; set; }

        // Group
        [ForeignKey(nameof(UserGroup))]
        public int GroupId { get; set; }
        public UserGroup? UserGroup { get; set; }

        // Schedule this user is sharing with this group
        public int? SharedScheduleId { get; set; }
        public Schedule? SharedSchedule { get; set; }
    }
}
