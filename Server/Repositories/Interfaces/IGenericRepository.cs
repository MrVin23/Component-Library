using System.Linq.Expressions;
using Server.Models;

namespace Server.Repositories.Interfaces
{
    public interface IGenericRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<PagedResponse<T>> GetPagedAsync(PaginationParameters parameters);
        Task<PagedResponse<T>> FindPagedAsync(Expression<Func<T, bool>> predicate, PaginationParameters parameters);
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        Task<T> AddAsync(T entity);
        Task AddRangeAsync(IEnumerable<T> entities);
        Task UpdateAsync(T entity);
        Task DeleteAsync(T entity);
        Task DeleteRangeAsync(IEnumerable<T> entities);
        Task<int> CountAsync(Expression<Func<T, bool>>? predicate = null);
        Task<bool> AnyAsync(Expression<Func<T, bool>> predicate);
        Task DeleteAllAsync();
        IQueryable<T> GetQueryable();
    }
}