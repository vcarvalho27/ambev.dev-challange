using Ambev.DeveloperEvaluation.Domain.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ambev.DeveloperEvaluation.Domain.Entities;

public class SaleProduct : BaseEntity
{
    [ForeignKey(nameof(SaleId))]
    public Guid SaleId { get; set; }
    public virtual Sale Sale { get; set; }

    [Required]
    public Guid ProductId { get; set; }
    [Required]
    public string ProductName { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int UnitPrice { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    // Discount percentage (0, 10 or 20) derived from Quantity - see SalePricingCalculator
    [Required]
    [Range(0, int.MaxValue)]
    public int Discount { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int TotalAmount { get; set; }

}
