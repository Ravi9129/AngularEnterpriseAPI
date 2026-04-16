using AngularEnterpriseAPI.Models.Entities;

namespace AngularEnterpriseAPI.Repositories.Interfaces
{
    public interface IRoleRepository
    {
        Task<Role> AddAsync(Role role);
        Task<Role?> GetByIdAsync(int id);
        Task<Role?> GetByNameAsync(string name);
        Task<IEnumerable<Role>> GetAllAsync();
    }
}
