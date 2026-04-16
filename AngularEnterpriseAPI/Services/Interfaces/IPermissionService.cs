using AngularEnterpriseAPI.DTOs.Permission;

namespace AngularEnterpriseAPI.Services.Interfaces
{
    public interface IPermissionService
    {
        Task<PermissionResponseDto> CreatePermissionAsync(CreatePermissionDto dto);
        Task<bool> AssignPermissionToUserAsync(AssignPermissionDto dto);
        Task<bool> AssignPermissionToRoleAsync(AssignPermissionDto dto);
        Task<bool> RemovePermissionFromUserAsync(AssignPermissionDto dto);
        Task<bool> RemovePermissionFromRoleAsync(AssignPermissionDto dto);
        Task<IEnumerable<PermissionResponseDto>> GetAllPermissionsAsync();
    }
}
