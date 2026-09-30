using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Scheducate.Models;

namespace Scheducate.Data
{
    public class ApplicationDbContext : IdentityDbContext<User>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Schedule> Schedule { get; set; } = default!;
        public DbSet<UserGroup> UserGroups { get; set; } = default!;
        public DbSet<GroupMember> GroupMembers { get; set; } = default!;
        public DbSet<GroupInvitation> GroupInvitations { get; set; } = default!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<GroupMember>()
                .HasOne(m => m.UserGroup)
                .WithMany(g => g.Members)
                .HasForeignKey(m => m.GroupId)
                .OnDelete(DeleteBehavior.NoAction);
        }
    }
}
