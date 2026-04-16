using System.ComponentModel.DataAnnotations;

namespace AngularEnterpriseAPI.DTOs.Role
{
    // Allow admin to identify the target user by Id, Username or Email.
    public class AssignRoleDto
    {
        // Provide one of these: UserId, Username or Email
        public int? UserId { get; set; }

        [MinLength(2)]
        [MaxLength(100)]
        public string? Username { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        [MinLength(2)]
        [MaxLength(50)]
        public string RoleName { get; set; } = string.Empty;
    }
}
