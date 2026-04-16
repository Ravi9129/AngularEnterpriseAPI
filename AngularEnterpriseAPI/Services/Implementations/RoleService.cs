using AngularEnterpriseAPI.DTOs.Role;
using AngularEnterpriseAPI.Models.Entities;
using AngularEnterpriseAPI.Repositories.Interfaces;
using AngularEnterpriseAPI.Services.Interfaces;
using AutoMapper;

namespace AngularEnterpriseAPI.Services.Implementations
{
    public class RoleService : IRoleService
    {
        private readonly IRoleRepository _roleRepository;
        private readonly IUserRoleRepository _userRoleRepository;
        private readonly IMapper _mapper;

        public RoleService(IRoleRepository roleRepository, IUserRoleRepository userRoleRepository, IMapper mapper)
        {
            _roleRepository = roleRepository;
            _userRoleRepository = userRoleRepository;
            _mapper = mapper;
        }

        public async Task<RoleResponseDto> CreateRoleAsync(CreateRoleDto dto)
        {
            var existing = await _roleRepository.GetByNameAsync(dto.Name);
            if (existing != null)
                throw new InvalidOperationException("Role already exists");

            var role = new Role
            {
                Name = dto.Name.ToUpperInvariant(),
                Description = dto.Description,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var created = await _roleRepository.AddAsync(role);
            return _mapper.Map<RoleResponseDto>(created);
        }

        public async Task<bool> AssignRoleToUserAsync(AssignRoleDto dto)
        {
            var role = await _roleRepository.GetByNameAsync(dto.RoleName.ToUpperInvariant());
            if (role == null)
                throw new KeyNotFoundException("Role not found");

            await _userRoleRepository.AssignRoleToUserAsync(dto.UserId, role.Id);
            return true;
        }

        public async Task<bool> RemoveRoleFromUserAsync(AssignRoleDto dto)
        {
            var role = await _roleRepository.GetByNameAsync(dto.RoleName.ToUpperInvariant());
            if (role == null)
                return false;

            return await _userRoleRepository.RemoveRoleFromUserAsync(dto.UserId, role.Id);
        }

        public async Task<IEnumerable<string>> GetRolesForUserAsync(int userId)
        {
            var roles = await _userRoleRepository.GetRolesByUserIdAsync(userId);
            return roles.Select(r => r.Name);
        }

        public async Task<IEnumerable<RoleResponseDto>> GetAllRolesAsync()
        {
            var roles = await _roleRepository.GetAllAsync();
            return roles.Select(r => _mapper.Map<RoleResponseDto>(r));
        }
    }
}
