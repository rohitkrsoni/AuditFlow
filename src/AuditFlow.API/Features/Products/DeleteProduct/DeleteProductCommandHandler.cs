using AuditFlow.API.Application.Abstractions.Messaging;
using AuditFlow.API.Application.Common.Errors;
using AuditFlow.API.Domain.Entities;
using AuditFlow.API.Infrastructure.Persistence;

using FluentResults;

using Microsoft.EntityFrameworkCore;

namespace AuditFlow.API.Features.Products.DeleteProduct;

internal sealed class DeleteProductCommandHandler : ICommandHandler<DeleteProductCommand>
{
    private readonly IApplicationDbContext _dbContext;

    public DeleteProductCommandHandler(IApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Result> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _dbContext.Products
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken);

        if (product is null)
        {
            return Result.Fail(ApplicationErrors.Common.NotFound(nameof(Product), request.Id.Value));
        }

        _dbContext.Products.Remove(product);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return Result.Ok();

    }
}
