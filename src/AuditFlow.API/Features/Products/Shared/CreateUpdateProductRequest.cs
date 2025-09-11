using AuditFlow.API.Domain.Enums;

using FluentValidation;

namespace AuditFlow.API.Features.Products.Shared;

/// <summary>
/// Request body for creating or updating a product.
/// </summary>
/// <param name="Name">The product name.</param>
/// <param name="Price">The product price.</param>
/// <param name="Description">A description of the product.</param>
/// <param name="ImageUrl">The product image URL.</param>
/// <param name="Category">The product category.</param>
/// <param name="Size">The product size.</param>
public sealed record CreateUpdateProductRequest(
  string Name,
  double Price,
  string Description,
  string ImageUrl,
  Category Category,
  Size Size
 );


internal sealed class CreateUpdateProductRequestValidator : AbstractValidator<CreateUpdateProductRequest>
{
    public CreateUpdateProductRequestValidator()
    {
        RuleFor(x => x.Name)
          .NotEmpty()
          .WithMessage("Product name is required.");

        RuleFor(x => x.Price)
          .GreaterThan(0)
          .WithMessage("Price must be greater than zero.");

        RuleFor(x => x.Description)
          .NotEmpty()
          .WithMessage("Description is required.");

        RuleFor(x => x.ImageUrl)
          .NotEmpty()
          .WithMessage("Image URL is required.");

        RuleFor(x => x.Category)
          .IsInEnum()
          .WithMessage("Invalid category specified.");

        RuleFor(x => x.Size)
          .IsInEnum()
          .WithMessage("Invalid size specified.");
    }
}
