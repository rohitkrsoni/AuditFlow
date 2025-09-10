using AuditFlow.API.Application.Abstractions.Messaging;
using AuditFlow.API.Domain.Entities;
using AuditFlow.API.Features.Common;
using AuditFlow.API.Features.Products.Shared;
using AuditFlow.API.Infrastructure.Persistence;
using AuditFlow.API.Shared;

using FluentResults;

using Microsoft.EntityFrameworkCore;

namespace AuditFlow.API.Features.Products.GetProducts;

internal sealed class GetProductsQueryHandler : IQueryHandler<GetProductsQuery, PaginatedResponse<CreateUpdateProductResponse>>
{
    private readonly IApplicationDbContext _dbContext;

    public GetProductsQueryHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result<PaginatedResponse<CreateUpdateProductResponse>>> Handle(GetProductsQuery query, CancellationToken cancellationToken)
    {
        var request = query.Request;

        var source = _dbContext.Products
            .AsNoTracking();

        if (!string.IsNullOrWhiteSpace(request.SearchName))
        {
            var term = request.SearchName.Trim();
            source = source.Where(p =>
                EF.Functions.Like(p.Name, $"%{term}%"));
        }

        source = source.OrderBy(p => p.Name);

        var paged = await PaginatedQueryResult<Product>.CreateAsync(
            source,
            pageNumber: request.PageNumber,
            pageSize: request.PageSize,
            cancellationToken);

        var items = paged.Select(p => new CreateUpdateProductResponse(
            p.Id.Value,
            p.Name,
            p.Price,
            p.Description,
            p.ImageUrl,
            p.Category,
            p.Size
        ));

        var response = new PaginatedResponse<CreateUpdateProductResponse>(
            items: items,
            pageIndex: paged.PageNumber,
            pageSize: paged.PageSize,
            totalRecords: paged.TotalRecords
        );

        return await Task.FromResult(Result.Ok(response));
    }
}
