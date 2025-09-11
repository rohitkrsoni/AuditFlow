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
    public GetProductQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }
    public async Task<Result<CreateUpdateProductResponse>> Handle(GetProductQuery request, CancellationToken cancellationToken)
    {
        var product = await _dbContext.Products
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

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
