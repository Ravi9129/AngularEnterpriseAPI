using System.ComponentModel.DataAnnotations;

namespace AngularEnterpriseAPI.Models.Entities
{
    public class PasswordResetToken
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int UserId { get; set; }

        public virtual User User { get; set; } = null!;

        [Required]
        public string Token { get; set; } = string.Empty;

        public DateTime ExpiryDate { get; set; }

        public bool IsUsed { get; set; } = false;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsValid => !IsUsed && DateTime.UtcNow <= ExpiryDate;
    }
}
