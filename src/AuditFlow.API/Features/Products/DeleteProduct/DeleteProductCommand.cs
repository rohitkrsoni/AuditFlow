using AuditFlow.API.Application.Abstractions.Messaging;
using AuditFlow.API.Domain.Entities;

using FluentValidation;

namespace AuditFlow.API.Features.Products.DeleteProduct;

public sealed record DeleteProductCommand(ProductId Id) : ICommand;

internal sealed class DeleteProductCommandValidator : AbstractValidator<DeleteProductCommand>
{
    public DeleteProductCommandValidator()
    {
        RuleFor(x => x.Id.Value).NotEmpty();
    }
}
