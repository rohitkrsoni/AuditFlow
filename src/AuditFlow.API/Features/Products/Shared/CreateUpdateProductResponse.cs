using AuditFlow.API.Domain.Enums;

namespace AuditFlow.API.Features.Products.Shared;

public sealed record CreateUpdateProductResponse(
  Guid Id,
  string Name,
  double Price,
  string Description,
  string ImageUrl,
  Category Category,
  Size Size
 );
