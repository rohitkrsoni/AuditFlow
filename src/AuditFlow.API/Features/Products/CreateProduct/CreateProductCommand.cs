using AuditFlow.API.Application.Abstractions.Messaging;
using AuditFlow.API.Features.Products.Shared;

using FluentValidation;

namespace AuditFlow.API.Features.Products.CreateProduct;

public sealed record CreateProductCommand(CreateUpdateProductRequest Request) : ICommand<CreateUpdateProductResponse>;

internal sealed class CreateProductCommandValidator : AbstractValidator<CreateProductCommand>
{
  public CreateProductCommandValidator()
  {
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
