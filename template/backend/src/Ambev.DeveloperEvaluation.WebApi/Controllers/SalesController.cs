using Ambev.DeveloperEvaluation.Application.Sales.CreateSale;
using Ambev.DeveloperEvaluation.Application.Sales.UpdateSale;
using Ambev.DeveloperEvaluation.Application.Sales.CancelSale;
using Ambev.DeveloperEvaluation.Application.Sales.CancelSaleItem;
using Ambev.DeveloperEvaluation.Application.Sales.GetSale;
using Ambev.DeveloperEvaluation.Application.Sales.ListSales;
using Ambev.DeveloperEvaluation.WebApi.Common;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.CreateSale;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.UpdateSale;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.CancelSale;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.CancelSaleItem;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.GetSale;
using Ambev.DeveloperEvaluation.WebApi.Features.Sales.ListSales;
using AutoMapper;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Ambev.DeveloperEvaluation.Common.Exceptions;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Ambev.DeveloperEvaluation.WebApi.Controllers;



/// <summary>
/// Controller for managing sales operations
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SalesController : BaseController
{
    private readonly IMediator _mediator;
    private readonly IMapper _mapper;

    /// <summary>
    /// Initializes a new instance of UsersController
    /// </summary>
    /// <param name="mediator">The mediator instance</param>
    /// <param name="mapper">The AutoMapper instance</param>
    public SalesController(IMediator mediator, IMapper mapper)
    {
        _mediator = mediator;
        _mapper = mapper;
    }

    /// <summary>
    /// Creates a new sale
    /// </summary>
    /// <param name="request">The sale creation request</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>The created sale details</returns>
    [HttpPost]
    [ProducesResponseType(typeof(ApiResponseWithData<CreateSaleResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Create([FromBody] CreateSaleRequest request, CancellationToken cancellationToken)
    {
        var validator = new CreateSaleRequestValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
            return BadRequest(new ApiResponse { Success = false, Message = "Validation failed", Errors = validationResult.Errors.Select(e => (global::Ambev.DeveloperEvaluation.Common.Validation.ValidationErrorDetail)e) });

        try
        {
            var command = _mapper.Map<CreateSaleCommand>(request);
            var response = await _mediator.Send(command, cancellationToken);

            return Created(string.Empty, new ApiResponseWithData<CreateSaleResponse>
            {
                Success = true,
                Message = "Sale created successfully",
                Data = _mapper.Map<CreateSaleResponse>(response)
            });
        }
        catch (UnprocessableEntityException ex)
        {
            return UnprocessableEntity(new ApiResponse { Success = false, Message = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(ApiResponseWithData<UpdateSaleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSaleRequest request, CancellationToken cancellationToken)
    {
        var validator = new UpdateSaleRequestValidator();
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
            return BadRequest(new ApiResponse { Success = false, Message = "Validation failed", Errors = validationResult.Errors.Select(e => (global::Ambev.DeveloperEvaluation.Common.Validation.ValidationErrorDetail)e) });

        try
        {
            var command = _mapper.Map<UpdateSaleCommand>(request);
            command.Id = id;
            var response = await _mediator.Send(command, cancellationToken);

            return Ok(new ApiResponseWithData<UpdateSaleResponse> { Success = true, Message = "Sale updated", Data = _mapper.Map<UpdateSaleResponse>(response) });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ApiResponse { Success = false, Message = ex.Message });
        }
        catch (UnprocessableEntityException ex)
        {
            return UnprocessableEntity(new ApiResponse { Success = false, Message = ex.Message });
        }

    }

    [HttpPost("{id}/cancel")]
    [ProducesResponseType(typeof(ApiResponseWithData<CancelSaleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Cancel(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var command = new CancelSaleCommand(id);
            var response = await _mediator.Send(command, cancellationToken);
            return Ok(new ApiResponseWithData<CancelSaleResponse> { Success = response.Success, Message = response.Message, Data = _mapper.Map<CancelSaleResponse>(response) });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ApiResponse { Success = false, Message = ex.Message });
        }
    }

    [HttpPost("{id}/items/{productId}/cancel")]
    [ProducesResponseType(typeof(ApiResponseWithData<CancelSaleItemResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CancelItem(Guid id, Guid productId, CancellationToken cancellationToken)
    {
        try 
        { 
            var command = new CancelSaleItemCommand(id, productId);
            var response = await _mediator.Send(command, cancellationToken);
            return Ok(new ApiResponseWithData<CancelSaleItemResponse> { Success = response.Success, Message = response.Message, Data = _mapper.Map<CancelSaleItemResponse>(response) });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ApiResponse { Success = false, Message = ex.Message });
        }
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponseWithData<GetSaleResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(Guid id, CancellationToken cancellationToken)
    {
        try 
        { 
            var command = new GetSaleCommand(id);
            var response = await _mediator.Send(command, cancellationToken);
            return Ok(new ApiResponseWithData<GetSaleResponse> { Success = true, Message = "Ok", Data = _mapper.Map<GetSaleResponse>(response) });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new ApiResponse { Success = false, Message = ex.Message });
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(ApiResponseWithData<IEnumerable<ListSaleItemResponse>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> List([FromQuery] int page = 1, [FromQuery] int size = 10, [FromQuery] string? order = null, CancellationToken cancellationToken = default)
    {
        // Parse order string like: "id desc, userId asc"
        List<Ambev.DeveloperEvaluation.Domain.Common.OrderBy> orders = new();
        if (!string.IsNullOrWhiteSpace(order))
        {
            var parts = order.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            foreach (var part in parts)
            {
                var seg = part.Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
                if (seg.Length == 0) continue;
                var field = seg[0];
                var dir = Ambev.DeveloperEvaluation.Domain.Common.Ordering.Asc;
                if (seg.Length > 1 && seg[1].Equals("desc", StringComparison.OrdinalIgnoreCase))
                    dir = Ambev.DeveloperEvaluation.Domain.Common.Ordering.Desc;

                orders.Add(new Ambev.DeveloperEvaluation.Domain.Common.OrderBy { Field = field, Direction = dir });
            }
        }

        var command = new ListSalesQuery(page, size, orders);
        var response = await _mediator.Send(command, cancellationToken);

        var mapped = _mapper.Map<IEnumerable<ListSaleItemResponse>>(response.Items);

        var paged = new PaginatedList<ListSaleItemResponse>(mapped.ToList(), response.TotalCount, response.Page, response.Size);
        return OkPaginated(paged);
    }
}
