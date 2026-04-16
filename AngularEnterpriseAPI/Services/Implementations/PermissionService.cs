using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using AngularEnterpriseAPI.DTOs.Permission;
using AngularEnterpriseAPI.Models.Entities;
using AngularEnterpriseAPI.Repositories.Interfaces;
using AngularEnterpriseAPI.Services.Interfaces;
using AutoMapper;

namespace AngularEnterpriseAPI.Services.Implementations
{
    public class PermissionService : IPermissionService
    {
        private readonly IRepository<Permission> _permissionRepo;
        private readonly IRepository<UserPermission> _userPermissionRepo;
        private readonly IRepository<RolePermission> _rolePermissionRepo;
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IMapper _mapper;

        public PermissionService(
            IRepository<Permission> permissionRepo,
            IRepository<UserPermission> userPermissionRepo,
            IRepository<RolePermission> rolePermissionRepo,
            IUserRepository userRepository,
            IRoleRepository roleRepository,
            IMapper mapper)
        {
            _permissionRepo = permissionRepo;
            _userPermissionRepo = userPermissionRepo;
            _rolePermissionRepo = rolePermissionRepo;
            _userRepository = userRepository;
            _roleRepository = roleRepository;
            _mapper = mapper;
        }

        public async Task<PermissionResponseDto> CreatePermissionAsync(CreatePermissionDto dto)
        {
            var name = dto.Name.Trim().ToUpperInvariant();
            var exists = await _permissionRepo.ExistsAsync(p => p.Name == name);
            if (exists)
                throw new InvalidOperationException("Permission already exists");

            var permission = new Permission
            {
                Name = name,
                Description = dto.Description,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

            var created = await _permissionRepo.AddAsync(permission);
            return _mapper.Map<PermissionResponseDto>(created);
        }

        public async Task<bool> AssignPermissionToUserAsync(AssignPermissionDto dto)
        {
            // resolve permission
            Permission? permission = null;
            if (dto.PermissionId.HasValue)
                permission = await _permissionRepo.GetByIdAsync(dto.PermissionId.Value);
            else if (!string.IsNullOrWhiteSpace(dto.PermissionName))
                permission = (await _permissionRepo.FindAsync(p => p.Name == dto.PermissionName.Trim().ToUpperInvariant())).FirstOrDefault();

            if (permission == null)
                throw new KeyNotFoundException("Permission not found");

            // resolve user
            User? user = null;
            if (dto.UserId.HasValue && dto.UserId.Value > 0)
                user = await _userRepository.GetByIdAsync(dto.UserId.Value);
            else if (!string.IsNullOrWhiteSpace(dto.Username))
                user = await _userRepository.GetByUsernameAsync(dto.Username);
            else if (!string.IsNullOrWhiteSpace(dto.Email))
                user = await _userRepository.GetByEmailAsync(dto.Email);

            if (user == null)
                throw new KeyNotFoundException("User not found");

            var exists = await _userPermissionRepo.ExistsAsync(up => up.UserId == user.Id && up.PermissionId == permission.Id);
            if (exists)
                return false;

            var userPerm = new UserPermission { UserId = user.Id, PermissionId = permission.Id, AssignedAt = DateTime.UtcNow };
            await _userPermissionRepo.AddAsync(userPerm);
            return true;
        }

        public async Task<bool> AssignPermissionToRoleAsync(AssignPermissionDto dto)
        {
            // resolve permission
            Permission? permission = null;
            if (dto.PermissionId.HasValue)
                permission = await _permissionRepo.GetByIdAsync(dto.PermissionId.Value);
            else if (!string.IsNullOrWhiteSpace(dto.PermissionName))
                permission = (await _permissionRepo.FindAsync(p => p.Name == dto.PermissionName.Trim().ToUpperInvariant())).FirstOrDefault();

            if (permission == null)
                throw new KeyNotFoundException("Permission not found");

            if (string.IsNullOrWhiteSpace(dto.RoleName))
                throw new ArgumentException("RoleName is required to assign permission to a role");

            var role = await _roleRepository.GetByNameAsync(dto.RoleName.Trim().ToUpperInvariant());
            if (role == null)
                throw new KeyNotFoundException("Role not found");

            var exists = await _rolePermissionRepo.ExistsAsync(rp => rp.RoleId == role.Id && rp.PermissionId == permission.Id);
            if (exists)
                return false;

            var rolePerm = new RolePermission { RoleId = role.Id, PermissionId = permission.Id, AssignedAt = DateTime.UtcNow };
            await _rolePermissionRepo.AddAsync(rolePerm);
            return true;
        }

        public async Task<bool> RemovePermissionFromUserAsync(AssignPermissionDto dto)
        {
            // resolve permission
            Permission? permission = null;
            if (dto.PermissionId.HasValue)
                permission = await _permissionRepo.GetByIdAsync(dto.PermissionId.Value);
            else if (!string.IsNullOrWhiteSpace(dto.PermissionName))
                permission = (await _permissionRepo.FindAsync(p => p.Name == dto.PermissionName.Trim().ToUpperInvariant())).FirstOrDefault();

            if (permission == null)
                return false;

            // resolve user
            User? user = null;
            if (dto.UserId.HasValue && dto.UserId.Value > 0)
                user = await _userRepository.GetByIdAsync(dto.UserId.Value);
            else if (!string.IsNullOrWhiteSpace(dto.Username))
                user = await _userRepository.GetByUsernameAsync(dto.Username);
            else if (!string.IsNullOrWhiteSpace(dto.Email))
                user = await _userRepository.GetByEmailAsync(dto.Email);

            if (user == null)
                return false;

            var matches = await _userPermissionRepo.FindAsync(up => up.UserId == user.Id && up.PermissionId == permission.Id);
            var entry = matches.FirstOrDefault();
            if (entry == null)
                return false;

            await _userPermissionRepo.DeleteAsync(entry);
            return true;
        }

        public async Task<bool> RemovePermissionFromRoleAsync(AssignPermissionDto dto)
        {
            Permission? permission = null;
            if (dto.PermissionId.HasValue)
                permission = await _permissionRepo.GetByIdAsync(dto.PermissionId.Value);
            else if (!string.IsNullOrWhiteSpace(dto.PermissionName))
                permission = (await _permissionRepo.FindAsync(p => p.Name == dto.PermissionName.Trim().ToUpperInvariant())).FirstOrDefault();

            if (permission == null)
                return false;

            if (string.IsNullOrWhiteSpace(dto.RoleName))
                return false;

            var role = await _roleRepository.GetByNameAsync(dto.RoleName.Trim().ToUpperInvariant());
            if (role == null)
                return false;

            var matches = await _rolePermissionRepo.FindAsync(rp => rp.RoleId == role.Id && rp.PermissionId == permission.Id);
            var entry = matches.FirstOrDefault();
            if (entry == null)
                return false;

            await _rolePermissionRepo.DeleteAsync(entry);
            return true;
        }

        public async Task<IEnumerable<PermissionResponseDto>> GetAllPermissionsAsync()
        {
            var perms = await _permissionRepo.GetAllAsync();
            return perms.Select(p => _mapper.Map<PermissionResponseDto>(p));
        }
    }
}
