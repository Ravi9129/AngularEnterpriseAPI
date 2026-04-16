using System.ComponentModel.DataAnnotations;

namespace AngularEnterpriseAPI.Models.Entities
{
    public class UserActivity
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        public virtual User User { get; set; } = null!;

        [Required]
        [MaxLength(50)]
        public string ActivityType { get; set; } = string.Empty; // Login, Logout, Create, Update, Delete

        [Required]
        [MaxLength(200)]
        public string Description { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? IpAddress { get; set; }

        [MaxLength(100)]
        public string? UserAgent { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public Dictionary<string, object>? Metadata { get; set; }
    }
}
