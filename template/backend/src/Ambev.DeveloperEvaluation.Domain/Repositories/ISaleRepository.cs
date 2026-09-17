using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ambev.DeveloperEvaluation.Domain.Repositories;

public interface ISaleRepository : IBaseRepository<Sale>
{
    /// <summary>
    /// Retrieves a sale together with its products
    /// </summary>
    /// <param name="id">The unique identifier of the sale</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The sale with its products loaded, or null if not found</returns>
    Task<Sale?> GetWithProductsAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Lists sales together with their products, paginated and optionally ordered
    /// </summary>
    Task<(IEnumerable<Sale> Items, int TotalCount)> ListWithProductsAsync(int page, int size, IEnumerable<OrderBy>? order = null, CancellationToken cancellationToken = default);

    Task<bool> ExistsSaleNumberAsync(string saleNumber, CancellationToken cancellationToken = default);
}
