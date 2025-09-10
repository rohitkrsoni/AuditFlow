using AuditFlow.API.Application.Abstractions.Messaging;
using AuditFlow.API.Application.Common.Errors;
using AuditFlow.API.Domain.Entities;
using AuditFlow.API.Features.Products.Shared;
using AuditFlow.API.Infrastructure.Persistence;

using FluentResults;

using Microsoft.EntityFrameworkCore;

namespace AuditFlow.API.Features.Products.UpdateProduct;

internal sealed class UpdateProductCommandHandler : ICommandHandler<UpdateProductCommand, CreateUpdateProductResponse>
{
    private readonly ILogger<UpdateProductCommandHandler> _logger;
    private readonly IApplicationDbContext _dbContext;

    public UpdateProductCommandHandler(IApplicationDbContext dbContext, ILogger<UpdateProductCommandHandler> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<Result<CreateUpdateProductResponse>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _dbContext.Products
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (product is null)
            return Result.Fail(ApplicationErrors.Common.NotFound(nameof(Product), request.Id.Value));

        UpdateProduct(product, request);

        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Ok(CreateResponse(product));
    }

    private static void UpdateProduct(Product product, UpdateProductCommand updateProductCommand)
    {
        product.Update(
            updateProductCommand.Request.Name,
            updateProductCommand.Request.Price,
            updateProductCommand.Request.Description,
            updateProductCommand.Request.ImageUrl,
            updateProductCommand.Request.Category,
            updateProductCommand.Request.Size
        );
    }

    private static CreateUpdateProductResponse CreateResponse(Product product)
    {
        return new CreateUpdateProductResponse(
            Id: product.Id.Value,
            Name: product.Name,
            Price: product.Price,
            Description: product.Description,
            ImageUrl: product.ImageUrl,
            Category: product.Category,
            Size: product.Size
        );
    }
}
