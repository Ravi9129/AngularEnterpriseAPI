using System.ComponentModel.DataAnnotations;

namespace AngularEnterpriseAPI.DTOs.User
{
    public class UpdateProfileDto
    {
        [MinLength(2)]
        [MaxLength(50)]
        public string? FirstName { get; set; }

        [MinLength(2)]
        [MaxLength(50)]
        public string? LastName { get; set; }

        [EmailAddress]
        [MaxLength(100)]
        public string? Email { get; set; }
    }
}
