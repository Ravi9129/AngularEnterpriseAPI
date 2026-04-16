using System.ComponentModel.DataAnnotations;

namespace AngularEnterpriseAPI.DTOs.Permission
{
    public class AssignPermissionDto
    {
        // Identify user by one of these (optional but at least one required when targeting a user)
        public int? UserId { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }

        // Identify role by name (optional but at least one of the user fields or RoleName must be provided)
        public string? RoleName { get; set; }

        // Permission can be referenced by Id or Name
        public int? PermissionId { get; set; }
        public string? PermissionName { get; set; }
    }
}
