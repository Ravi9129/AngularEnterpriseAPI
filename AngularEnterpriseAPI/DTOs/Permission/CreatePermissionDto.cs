using System.ComponentModel.DataAnnotations;

namespace AngularEnterpriseAPI.DTOs.Permission
{
    public class CreatePermissionDto
    {
        [Required]
        [MinLength(2)]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(250)]
        public string? Description { get; set; }
    }
}
