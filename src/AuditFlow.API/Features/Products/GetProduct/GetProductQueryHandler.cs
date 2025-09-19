using AuditFlow.API.Application.Abstractions.Messaging;
using AuditFlow.API.Application.Common.Errors;
using AuditFlow.API.Domain.Entities;
using AuditFlow.API.Features.Products.Shared;
using AuditFlow.API.Infrastructure.Persistence;

using FluentResults;

using Microsoft.EntityFrameworkCore;

namespace AuditFlow.API.Features.Products.GetProduct;

public class GetProductQueryHandler : IQueryHandler<GetProductQuery, CreateUpdateProductResponse>
{
    private readonly IApplicationDbContext _dbContext;
    private readonly ILogger<GetProductQueryHandler> _logger;
    public GetProductQueryHandler(IApplicationDbContext dbContext, ILogger<GetProductQueryHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }
    public async Task<Result<CreateUpdateProductResponse>> Handle(GetProductQuery request, CancellationToken cancellationToken)
    {
        var product = await _dbContext.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (product == null) {
            _logger.LogWarning("Product with ID {ProductId} not found.", request.Id.Value);
        } else {
            _logger.LogInformation("Product with ID {ProductId} retrieved successfully.", request.Id.Value);
        }

        return product is null
            ? Result.Fail<CreateUpdateProductResponse>(
                ApplicationErrors.Common.NotFound(nameof(Product), request.Id.Value))
            : Result.Ok(new CreateUpdateProductResponse(
            product.Id.Value,
            product.Name,
            product.Price,
            product.Description,
            product.ImageUrl,
            product.Category,
            product.Size
        ));
    }
}
