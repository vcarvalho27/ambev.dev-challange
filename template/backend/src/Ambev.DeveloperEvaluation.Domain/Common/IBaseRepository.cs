using Ambev.DeveloperEvaluation.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ambev.DeveloperEvaluation.Domain.Common;

public interface IBaseRepository<T>
{
    /// <summary>
    /// Creates a new object in the repository
    /// </summary>
    /// <param name="obj">The object to create</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created object</returns>
    Task<T> CreateAsync(T obj, CancellationToken cancellationToken = default);

    /// <summary>
    /// Updates an existing object in the repository
    /// </summary>
    /// <param name="obj">The object to update</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The updated object</returns>
    Task<T> UpdateAsync(T obj, CancellationToken cancellationToken = default);


    /// <summary>
    /// Retrieves an object by its unique identifier
    /// </summary>
    /// <param name="id">The unique identifier of the object</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The object if found, null otherwise</returns>
    Task<T?> GetAsync(Guid id, CancellationToken cancellationToken = default);


    /// <summary>
    /// Checks if an object exists by its unique identifier
    /// </summary>
    /// <param name="id">The unique identifier of the object</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The object if found, null otherwise</returns>
    Task<bool> Exists(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves a list of objects
    /// </summary>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>A list of objects</returns>
    Task<IEnumerable<T>> ListAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists a collection of objects with pagination and ordering
    /// </summary>
    Task<(IEnumerable<T> Items, int TotalCount)> ListAsync(int page, int size, IEnumerable<Ambev.DeveloperEvaluation.Domain.Common.OrderBy>? order = null, CancellationToken cancellationToken = default);

    /// <summary>
    /// Soft deletes an object from the repository
    /// </summary>
    /// <param name="id">The unique identifier of the object to delete</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>True if the object was deleted, false if not found</returns>
    Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken = default);
}
