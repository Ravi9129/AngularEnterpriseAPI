using System.ComponentModel.DataAnnotations;
using AngularEnterpriseAPI.Models.Enums;

namespace AngularEnterpriseAPI.DTOs.User
{
    public class CreateUserDto
    {
        [Required]
        [MinLength(3)]
        [MaxLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [MaxLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [MinLength(2)]
        [MaxLength(50)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [MinLength(2)]
        [MaxLength(50)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        [MinLength(6)]
        [MaxLength(20)]
        public string Password { get; set; } = string.Empty;

        public UserRole Role { get; set; } = UserRole.USER;
    }
}
