using AngularEnterpriseAPI.DTOs.Role;

namespace AngularEnterpriseAPI.Services.Interfaces
{
    public interface IRoleService
    {
        Task<RoleResponseDto> CreateRoleAsync(CreateRoleDto dto);
        Task<bool> AssignRoleToUserAsync(AssignRoleDto dto);
        Task<bool> RemoveRoleFromUserAsync(AssignRoleDto dto);
        Task<IEnumerable<string>> GetRolesForUserAsync(int userId);
        Task<IEnumerable<RoleResponseDto>> GetAllRolesAsync();
    }
}
