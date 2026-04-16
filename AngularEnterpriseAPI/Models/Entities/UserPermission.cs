using System;

namespace AngularEnterpriseAPI.Models.Entities
{
    public class UserPermission
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int PermissionId { get; set; }
        public DateTime AssignedAt { get; set; } = DateTime.UtcNow;

        public User? User { get; set; }
        public Permission? Permission { get; set; }
    }
}
