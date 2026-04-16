using System;
using System.Collections.Generic;
using AngularEnterpriseAPI.DTOs.Common;
using AngularEnterpriseAPI.DTOs.User;

namespace AngularEnterpriseAPI.Services.Interfaces
{
    public interface IUserService
    {
        Task<UserResponseDto?> GetUserByIdAsync(int id);
        Task<UserResponseDto?> GetUserByUsernameAsync(string username);
        Task<PagedResponse<UserResponseDto>> GetPagedUsersAsync(int pageNumber, int pageSize, string? filter = null, string? sortBy = null);
        Task<UserResponseDto> CreateUserAsync(CreateUserDto createUserDto);
        Task<UserResponseDto?> UpdateUserAsync(int id, UpdateUserDto updateUserDto);
        Task<bool> DeleteUserAsync(int id);
        Task<bool> UserExistsAsync(string username, string email);
        // Dashboard related counts
        Task<int> GetTotalUsersCountAsync();
        Task<int> GetActiveUsersCountAsync();
        Task<int> GetNewUsersCountAsync(DateTime since);
        Task<Dictionary<string, int>> GetUsersCountByRoleAsync();
    }
}
