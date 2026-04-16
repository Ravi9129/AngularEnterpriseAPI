using System.ComponentModel.DataAnnotations;

namespace AngularEnterpriseAPI.DTOs.Role
{
    public class AssignRoleDto
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        [MinLength(2)]
        [MaxLength(50)]
        public string RoleName { get; set; } = string.Empty;
    }
}
