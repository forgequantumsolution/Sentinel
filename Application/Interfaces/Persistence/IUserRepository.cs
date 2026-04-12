using System.Linq.Expressions;
using Core.Entities;

namespace Application.Interfaces.Persistence
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(Guid id);
        Task<User?> GetByEmailAsync(string email);
        Task<List<User>> GetAllAsync();
        Task AddAsync(User user);
        Task UpdateAsync(User user);
        Task DeleteAsync(Guid id);
        Task<bool> ExistsAsync(string email);
        Task<bool> AnyMatchAsync(Expression<Func<User, bool>> predicate);
        Task<bool> AnyMatchAsync(Guid userId, IEnumerable<Expression<Func<User, bool>>> predicates);
        Task<List<User>> FindAsync(Expression<Func<User, bool>> predicate);
        Task<List<User>> FindAsync(IEnumerable<Expression<Func<User, bool>>> predicates);
    }
}
