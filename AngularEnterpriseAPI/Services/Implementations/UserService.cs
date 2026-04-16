using System;
using System.Collections.Generic;
using System.Linq;
using AngularEnterpriseAPI.DTOs.Activity;
using AngularEnterpriseAPI.DTOs.Common;
using AngularEnterpriseAPI.DTOs.User;
using AngularEnterpriseAPI.Models.Entities;
using AngularEnterpriseAPI.Repositories.Interfaces;
using AngularEnterpriseAPI.Services.Interfaces;
using AutoMapper;

namespace AngularEnterpriseAPI.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public UserService(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<UserResponseDto?> GetUserByIdAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            return user != null ? _mapper.Map<UserResponseDto>(user) : null;
        }

        public async Task<UserResponseDto?> GetUserByUsernameAsync(string username)
        {
            var user = await _userRepository.GetByUsernameAsync(username);
            return user != null ? _mapper.Map<UserResponseDto>(user) : null;
        }

        public async Task<PagedResponse<UserResponseDto>> GetPagedUsersAsync(
            int pageNumber,
            int pageSize,
            string? filter = null,
            string? sortBy = null)
        {
            var (users, totalCount) = await _userRepository.GetPagedUsersAsync(
                pageNumber, pageSize, filter, sortBy);

            var userDtos = _mapper.Map<IEnumerable<UserResponseDto>>(users);

            return new PagedResponse<UserResponseDto>
            {
                Data = userDtos,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalRecords = totalCount,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };
        }

        public async Task<UserResponseDto> CreateUserAsync(CreateUserDto createUserDto)
        {
            // Check if user exists
            var exists = await _userRepository.ExistsAsync(u =>
                u.Username == createUserDto.Username ||
                u.Email == createUserDto.Email);

            if (exists)
                throw new InvalidOperationException("Username or email already exists");

            var user = _mapper.Map<User>(createUserDto);
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(createUserDto.Password);
            user.CreatedAt = DateTime.UtcNow;
            user.IsActive = true;

            var createdUser = await _userRepository.AddAsync(user);
            return _mapper.Map<UserResponseDto>(createdUser);
        }

        public async Task<UserResponseDto?> UpdateUserAsync(int id, UpdateUserDto updateUserDto)
        {
            var user = await _userRepository.GetByIdAsync(id);

            if (user == null)
                return null;

            // Update only provided fields
            if (!string.IsNullOrWhiteSpace(updateUserDto.FirstName))
                user.FirstName = updateUserDto.FirstName;

            if (!string.IsNullOrWhiteSpace(updateUserDto.LastName))
                user.LastName = updateUserDto.LastName;

            if (!string.IsNullOrWhiteSpace(updateUserDto.Email))
                user.Email = updateUserDto.Email;

            if (updateUserDto.Role.HasValue)
                user.Role = updateUserDto.Role.Value;

            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);
            return _mapper.Map<UserResponseDto>(user);
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);

            if (user == null)
                return false;

            // Soft delete
            user.IsActive = false;
            user.UpdatedAt = DateTime.UtcNow;

            await _userRepository.UpdateAsync(user);
            return true;
        }

        public async Task<bool> UserExistsAsync(string username, string email)
        {
            return await _userRepository.ExistsAsync(u =>
                u.Username == username || u.Email == email);
        }





        // Add these methods to existing UserService class

        public async Task<int> GetTotalUsersCountAsync()
        {
            return await _userRepository.CountAsync(u => u.IsActive);
        }

        public async Task<int> GetActiveUsersCountAsync()
        {
            return await _userRepository.CountAsync(u => u.IsActive && u.IsActive);
        }

        public async Task<int> GetNewUsersCountAsync(DateTime since)
        {
            return await _userRepository.CountAsync(u => u.CreatedAt >= since && u.IsActive);
        }

        public async Task<Dictionary<string, int>> GetUsersCountByRoleAsync()
        {
            var users = await _userRepository.GetAllAsync();
            return users
                .Where(u => u.IsActive)
                .GroupBy(u => u.Role.ToString())
                .ToDictionary(g => g.Key, g => g.Count());
        }

        public async Task<IEnumerable<ActivityDto>> GetRecentActivitiesAsync(int limit)
        {
            // This would typically come from an activity service
            // For now, return mock data
            var activities = new List<ActivityDto>
    {
        new() { Id = 1, Title = "User logged in", Type = "Login", Icon = "login", Timestamp = DateTime.UtcNow },
        new() { Id = 2, Title = "User created", Type = "Create", Icon = "person_add", Timestamp = DateTime.UtcNow.AddHours(-1) },
        new() { Id = 3, Title = "User updated", Type = "Update", Icon = "edit", Timestamp = DateTime.UtcNow.AddHours(-2) }
    };
            return activities.Take(limit);
        }
    }
}
