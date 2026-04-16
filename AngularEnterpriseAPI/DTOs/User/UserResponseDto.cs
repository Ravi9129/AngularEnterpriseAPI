using AngularEnterpriseAPI.Models.Enums;

namespace AngularEnterpriseAPI.DTOs.User
{
    public class UserResponseDto
    {
        public int Id { get; set; }
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string FullName => $"{FirstName} {LastName}";
    }
}
