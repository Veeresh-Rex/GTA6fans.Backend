using System.Linq.Expressions;
using GTA6fans.Domain.Entities;
using GTA6fans.Domain.Models;

namespace GTA6fans.Domain.Interfaces;

public interface IGenericRepository<T> where T : BaseEntity
{
    Task<IEnumerable<T>> GetAllAsync();
    Task<T?> GetByIdAsync(string id);
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
    Task<List<TResult>> FindAsync<TResult>(Expression<Func<T, bool>> predicate, Expression<Func<T, TResult>> projection);
    Task<T?> FindFirstOrDefaultAsync(Expression<Func<T, bool>> predicate);
    Task<PagedResult<T>> GetPagedAsync(int pageNumber, int pageSize, Expression<Func<T, bool>>? predicate = null, Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null);
    Task<T> CreateAsync(T entity);
    Task<T> UpdateAsync(T entity);
    Task<bool> DeleteAsync(string id);
    Task<bool> DeleteAsync(T entity);
    Task<long> CountAsync();
    Task<long> CountAsync(Expression<Func<T, bool>> predicate);
    Task<bool> ExistsAsync(string id);
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate);
}