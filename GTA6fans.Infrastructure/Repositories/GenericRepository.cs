using System;
using System.Linq.Expressions;
using GTA6fans.Domain.Entities;
using GTA6fans.Domain.Interfaces;
using GTA6fans.Domain.Models;
using GTA6fans.Infrastructure.Data;
using MongoDB.Driver;
using MongoDB.Driver.Linq;

namespace GTA6fans.Infrastructure.Repositories;

public class GenericRepository<T> : IGenericRepository<T> where T : BaseEntity
{
    protected readonly IMongoCollection<T> _collection;
    protected readonly MongoDbContext _context;

    public GenericRepository(MongoDbContext context)
    {
        _context = context;
        var collectionName = GetCollectionName();
        _collection = _context.GetCollection<T>(collectionName);
    }

    public virtual async Task<IEnumerable<T>> GetAllAsync()
    {
        return await _collection.Find(x => !x.IsDeleted).ToListAsync();
    }

    public virtual async Task<T?> GetByIdAsync(string id)
    {
        return await _collection.Find(x => x.Id == id && !x.IsDeleted).FirstOrDefaultAsync();
    }

    public virtual async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
    {
        var filter = Builders<T>.Filter.And(
            Builders<T>.Filter.Where(predicate),
            Builders<T>.Filter.Eq(x => x.IsDeleted, false)
        );
        return await _collection.Find(filter).ToListAsync();
    }

    public async Task<List<TResult>> FindAsync<TResult>(Expression<Func<T, bool>> predicate, Expression<Func<T, TResult>> projection)
    {
        var filter = Builders<T>.Filter.And(Builders<T>.Filter.Where(predicate), Builders<T>.Filter.Eq(x => x.IsDeleted, false)
);
        return await _collection.Find(filter).Project(projection).ToListAsync();
    }

    public async Task<T?> FindFirstOrDefaultAsync(Expression<Func<T, bool>> predicate)
    {
        var filter = Builders<T>.Filter.And(Builders<T>.Filter.Where(predicate), Builders<T>.Filter.Eq(x => x.IsDeleted, false));
        return await _collection.Find(filter).FirstOrDefaultAsync();
    }

    public virtual async Task<T> CreateAsync(T entity)
    {
        entity.CreatedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        await _collection.InsertOneAsync(entity);
        return entity;
    }

    public virtual async Task<T> UpdateAsync(T entity)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        await _collection.ReplaceOneAsync(x => x.Id == entity.Id, entity);
        return entity;
    }

    public virtual async Task<bool> DeleteAsync(string id)
    {
        var filter = Builders<T>.Filter.Eq(x => x.Id, id);
        var update = Builders<T>.Update
            .Set(x => x.IsDeleted, true)
            .Set(x => x.UpdatedAt, DateTime.UtcNow);
        
        var result = await _collection.UpdateOneAsync(filter, update);
        return result.ModifiedCount > 0;
    }

    public virtual async Task<bool> DeleteAsync(T entity)
    {
        return await DeleteAsync(entity.Id);
    }

    public virtual async Task<long> CountAsync()
    {
        return await _collection.CountDocumentsAsync(x => !x.IsDeleted);
    }

    public virtual async Task<long> CountAsync(Expression<Func<T, bool>> predicate)
    {
        var filter = Builders<T>.Filter.And(
            Builders<T>.Filter.Where(predicate),
            Builders<T>.Filter.Eq(x => x.IsDeleted, false)
        );
        return await _collection.CountDocumentsAsync(filter);
    }

    public virtual async Task<bool> ExistsAsync(string id)
    {
        return await _collection.Find(x => x.Id == id && !x.IsDeleted).AnyAsync();
    }

    public virtual async Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate)
    {
        var filter = Builders<T>.Filter.And(
            Builders<T>.Filter.Where(predicate),
            Builders<T>.Filter.Eq(x => x.IsDeleted, false)
        );
        return await _collection.Find(filter).AnyAsync();
    }

    private string GetCollectionName()
    {
        var attribute = typeof(T).GetCustomAttributes(typeof(BsonCollectionAttribute), true)
            .FirstOrDefault() as BsonCollectionAttribute;
        
        return attribute?.CollectionName ?? typeof(T).Name.ToLower();
    }


    public async Task<PagedResult<T>> GetPagedAsync(int pageNumber, int pageSize, Expression<Func<T, bool>>? predicate = null, Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null)
    {
        if (pageNumber < 1) throw new ArgumentException("Page number must be greater than 0", nameof(pageNumber));
        if (pageSize < 1) throw new ArgumentException("Page size must be greater than 0", nameof(pageSize));
        IQueryable<T> query = _collection.AsQueryable();

        try
        {
            var filter = predicate != null ? Builders<T>.Filter.And(
            Builders<T>.Filter.Where(predicate),
            Builders<T>.Filter.Eq(x => x.IsDeleted, false)) : 
            FilterDefinition<T>.Empty;

            var totalCount = await _collection.CountDocumentsAsync(filter);
            var skip = (pageNumber - 1) * pageSize;

            if (orderBy != null)
            {
                query = orderBy(query);
            }

            if (predicate != null)
            {
                query = query.Where(predicate);
            }

            var items = await query.Skip(skip).Take(pageSize).ToListAsync();

            return new PagedResult<T>
            {
                Items = items,
                TotalCount = totalCount,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
            };
        }
        catch (Exception)
        {
            throw;
        }
    }

}
