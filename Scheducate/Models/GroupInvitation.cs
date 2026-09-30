using System.ComponentModel.DataAnnotations.Schema;

namespace Scheducate.Models
{
    public class GroupInvitation
    {
        public int Id { get; set; }

        // Group being invited to
        [ForeignKey(nameof(UserGroup))]
        public int GroupId { get; set; }
        public UserGroup? UserGroup { get; set; }

        // Unique token used in the invitation URL
        public string Token { get; set; } = null!;

        // When the invitation was created
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}