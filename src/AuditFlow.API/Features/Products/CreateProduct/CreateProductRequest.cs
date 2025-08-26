using AuditFlow.API.Application.Abstractions.Messaging;
using AuditFlow.API.Features.Products.Shared;

using FluentValidation;

namespace AuditFlow.API.Features.Products.CreateProduct;

public sealed record CreateProductRequest(CreateUpdateProductRequest Request) : ICommand<CreateUpdateProductResponse>;

internal sealed class CreateProductRequestValidator : AbstractValidator<CreateProductRequest>
{
  public CreateProductRequestValidator()
  {
    RuleFor(x => x.Request.Name)
      .NotEmpty()
      .WithMessage("Product name is required.");

    RuleFor(x => x.Request.Price)
      .GreaterThan(0)
      .WithMessage("Price must be greater than zero.");

    RuleFor(x => x.Request.Description)
      .NotEmpty()
      .WithMessage("Description is required.");

    RuleFor(x => x.Request.ImageUrl)
      .NotEmpty()
      .WithMessage("Image URL is required.");

    RuleFor(x => x.Request.Category)
      .IsInEnum()
      .WithMessage("Invalid category specified.");

    RuleFor(x => x.Request.Size)
      .IsInEnum()
      .WithMessage("Invalid size specified.");
  }
}
