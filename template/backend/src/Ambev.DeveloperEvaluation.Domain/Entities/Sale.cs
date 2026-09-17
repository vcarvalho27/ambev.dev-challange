using Ambev.DeveloperEvaluation.Common.Security;
using Ambev.DeveloperEvaluation.Domain.Common;
using Ambev.DeveloperEvaluation.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Ambev.DeveloperEvaluation.Domain.Entities;

public class Sale : BaseEntity
{
    [Required]
    public string SaleNumber { get; set; }

    [Required]
    public Guid CustomerId { get; set; }
    [Required]
    public string CustomerName { get; set; }


    [Required]
    public Guid BranchId { get; set; }
    [Required]
    public string BranchName { get; set; }

    //Subtotal, discount and total are registered in cents to avoid rounding problems
    //Subtotal, discount and total are registered in database to increase performance and avoid calculations in queries

    [Required]
    [Range(1, int.MaxValue)]
    public int Subtotal { get; set; }

    [Range(0, int.MaxValue)]
    public int Discount { get; set; }

    [Range(1, int.MaxValue)]
    public int Total { get; set; }

    public DateTime? CancelledAt { get; set; }


    public virtual ICollection<SaleProduct> SaleProducts { get; set; }

}
