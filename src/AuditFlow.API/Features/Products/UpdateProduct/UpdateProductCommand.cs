using AuditFlow.API.Application.Abstractions.Messaging;
using AuditFlow.API.Domain.Entities;
using AuditFlow.API.Features.Products.Shared;

using FluentValidation;

namespace AuditFlow.API.Features.Products.UpdateProduct;

public sealed record UpdateProductCommand(ProductId Id, CreateUpdateProductRequest Request) : ICommand<CreateUpdateProductResponse>;

internal sealed class UpdateProductCommandValidator : AbstractValidator<UpdateProductCommand>
{
    public UpdateProductCommandValidator()
    {
        RuleFor(x => x.Id.Value).NotEmpty();

        RuleFor(x => x.Request.Name).NotEmpty();

        RuleFor(x => x.Request.Price)
          .GreaterThan(0)
          .WithMessage("Product price must be greater than zero.");

        RuleFor(x => x.Request.Description).NotEmpty();

        RuleFor(x => x.Request.ImageUrl).NotEmpty();

        RuleFor(x => x.Request.Category)
          .IsInEnum()
          .WithMessage("Invalid category specified.");

        RuleFor(x => x.Request.Size)
          .IsInEnum()
          .WithMessage("Invalid size specified.");
    }
}
