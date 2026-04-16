using System;
using System.Collections.Generic;

namespace AngularEnterpriseAPI.Models.Entities
{
    public class Permission
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<UserPermission>? UserPermissions { get; set; }
        public ICollection<RolePermission>? RolePermissions { get; set; }
    }
}
