using Microsoft.AspNetCore.Identity;
namespace Scheducate.Models
{
    public class User : IdentityUser
    {
        public ICollection<Schedule> Schedules { get; set; } = [];
    }
}
