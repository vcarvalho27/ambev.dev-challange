using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ambev.DeveloperEvaluation.ORM.Common;

public class BaseRepository<T> : IBaseRepository<T> where T : BaseEntity
{
    protected readonly DefaultContext _context;

    /// <summary>
    /// Initializes a new instance of UserRepository
    /// </summary>
    /// <param name="context">The database context</param>
    public BaseRepository(DefaultContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Creates a new object in the database
    /// </summary>
    /// <param name="obj">The object to create</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created object</returns>
    public async Task<T> CreateAsync(T obj, CancellationToken cancellationToken = default)
    {
        await _context.Set<T>().AddAsync(obj, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);
        return obj;
    }


    /// <summary>
    /// Updates an existing object in the database
    /// </summary>
    /// <param name="obj">The object to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated object</returns>
    public async Task<T> UpdateAsync(T obj, CancellationToken cancellationToken = default)
    {
        _context.Set<T>().Update(obj);
        await _context.SaveChangesAsync(cancellationToken);
        return obj;
    }

    /// <summary>
    /// Retrieves an object by its unique identifier
    /// </summary>
    /// <param name="id">The unique identifier of the object</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The object if found, null otherwise</returns>
    public async Task<T?> GetAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Set<T>().FirstOrDefaultAsync(o => o.Id == id && o.DeletedAt == null, cancellationToken);
    }


    /// <summary>
    /// Checks if an object exists by its unique identifier
    /// </summary>
    /// <param name="id">The unique identifier of the object</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The object if found, null otherwise</returns>
    public async Task<bool> Exists(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Set<T>().AnyAsync(o => o.Id == id && o.DeletedAt == null, cancellationToken);
    }

    /// <summary>
    /// Builds a queryable list of objects that are not deleted
    /// Overridable method to allow customization in derived classes
    /// </summary>
    /// <returns></returns>
    protected IQueryable<T> BuildList()
    {
        return _context.Set<T>().Where(o => o.DeletedAt == null);
    }

    /// <summary>
    /// Lists a collection of objects
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The list of objects</returns>
    public async Task<IEnumerable<T>> ListAsync(CancellationToken cancellationToken = default)
    {
        return await BuildList().AsNoTracking().ToListAsync(cancellationToken);
    }

    public async Task<(IEnumerable<T> Items, int TotalCount)> ListAsync(int page, int size, IEnumerable<Ambev.DeveloperEvaluation.Domain.Common.OrderBy>? order = null, CancellationToken cancellationToken = default)
    {
        return await ApplyOrderAndPageAsync(BuildList(), page, size, order, cancellationToken);
    }

    /// <summary>
    /// Applies dynamic ordering and pagination to a query, returning the page of items and the total count.
    /// Allows derived repositories to reuse the ordering/paging logic on top of a custom base query (e.g. one with .Include()).
    /// </summary>
    protected async Task<(IEnumerable<T> Items, int TotalCount)> ApplyOrderAndPageAsync(IQueryable<T> query, int page, int size, IEnumerable<Ambev.DeveloperEvaluation.Domain.Common.OrderBy>? order, CancellationToken cancellationToken)
    {
        // Apply ordering if specified
        if (order != null && order.Any())
        {
            IOrderedQueryable<T>? orderedQuery = null;
            foreach (var ord in order)
            {
                var prop = typeof(T).GetProperty(ord.Field, System.Reflection.BindingFlags.IgnoreCase | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (prop == null)
                    continue;

                var param = System.Linq.Expressions.Expression.Parameter(typeof(T), "x");
                var propertyAccess = System.Linq.Expressions.Expression.Property(param, prop);
                var lambda = System.Linq.Expressions.Expression.Lambda(propertyAccess, param);

                string methodName;
                if (orderedQuery == null)
                {
                    methodName = ord.Direction == Ambev.DeveloperEvaluation.Domain.Common.Ordering.Desc ? "OrderByDescending" : "OrderBy";
                    var resultExp = System.Linq.Expressions.Expression.Call(typeof(Queryable), methodName, new Type[] { typeof(T), prop.PropertyType }, query.Expression, System.Linq.Expressions.Expression.Quote(lambda));
                    query = query.Provider.CreateQuery<T>(resultExp);
                    orderedQuery = (IOrderedQueryable<T>)query;
                }
                else
                {
                    methodName = ord.Direction == Ambev.DeveloperEvaluation.Domain.Common.Ordering.Desc ? "ThenByDescending" : "ThenBy";
                    var resultExp = System.Linq.Expressions.Expression.Call(typeof(Queryable), methodName, new Type[] { typeof(T), prop.PropertyType }, orderedQuery.Expression, System.Linq.Expressions.Expression.Quote(lambda));
                    orderedQuery = (IOrderedQueryable<T>)orderedQuery.Provider.CreateQuery<T>(resultExp);
                    query = orderedQuery;
                }
            }
        }

        var total = await query.CountAsync(cancellationToken);
        var items = await query.Skip((page - 1) * size).Take(size).AsNoTracking().ToListAsync(cancellationToken);
        return (items, total);
    }

    /// <summary>
    /// Deletes a user from the database
    /// </summary>
    /// <param name="id">The unique identifier of the user to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the user was deleted, false if not found</returns>
    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var obj = await GetAsync(id, cancellationToken);
        if (obj == null)
            return false;

        //_context.Set<T>().Remove(obj); I`m not using hard delete
        
        obj.DeletedAt = DateTime.UtcNow;
        _context.Set<T>().Update(obj);

        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }
}
