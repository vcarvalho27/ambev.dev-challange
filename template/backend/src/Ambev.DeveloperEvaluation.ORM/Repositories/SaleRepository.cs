using Ambev.DeveloperEvaluation.Domain.Entities;
using Ambev.DeveloperEvaluation.Domain.Repositories;
using Ambev.DeveloperEvaluation.ORM.Common;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Ambev.DeveloperEvaluation.ORM.Repositories;

public class SaleRepository : BaseRepository<Sale>, ISaleRepository
{
    public SaleRepository(DefaultContext context) : base(context)
    {
    }

    
    public async Task<bool> ExistsSaleNumberAsync(string saleNumber, CancellationToken cancellationToken = default)
    {
        return await _context.Sales.AnyAsync(s => s.SaleNumber == saleNumber, cancellationToken);
    }

    public async Task<Sale?> GetWithProductsAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await _context.Sales
            .Include(s => s.SaleProducts)
            .FirstOrDefaultAsync(s => s.Id == id && s.DeletedAt == null, cancellationToken);
    }

    public async Task<(IEnumerable<Sale> Items, int TotalCount)> ListWithProductsAsync(int page, int size, IEnumerable<Ambev.DeveloperEvaluation.Domain.Common.OrderBy>? order = null, CancellationToken cancellationToken = default)
    {
        var query = _context.Sales
            .Include(s => s.SaleProducts)
            .Where(s => s.DeletedAt == null);

        return await ApplyOrderAndPageAsync(query, page, size, order, cancellationToken);
    }
}
