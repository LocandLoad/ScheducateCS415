using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Scheducate.Models
{
    public class Schedule
    {
        public int Id { get; set; }
        public required string Name { get; set; }

        [ForeignKey("User")]
        [ValidateNever]
        public string UserId { get; set; } = null!;
        public User? User { get; set; }
        public byte[] Availability { get; set; } = new byte[42]; // 7 * 48 (7 days 30 minute intervals)
    }
}
