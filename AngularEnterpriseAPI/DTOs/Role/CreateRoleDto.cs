using System.ComponentModel.DataAnnotations;

namespace AngularEnterpriseAPI.DTOs.Role
{
    public class CreateRoleDto
    {
        [Required]
        [MinLength(2)]
        [MaxLength(50)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? Description { get; set; }
    }
}
