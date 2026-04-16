using AngularEnterpriseAPI.Models.Entities;

namespace AngularEnterpriseAPI.Repositories.Interfaces
{
    public interface IUserRepository : IRepository<User>
    {
        Task<User?> GetByUsernameAsync(string username);
        Task<User?> GetByEmailAsync(string email);
        Task<IEnumerable<User>> GetUsersByRoleAsync(string role);
        Task<(IEnumerable<User> Users, int TotalCount)> GetPagedUsersAsync(int pageNumber, int pageSize, string? filter = null, string? sortBy = null);
    }
}
