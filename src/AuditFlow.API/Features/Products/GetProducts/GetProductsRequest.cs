namespace AuditFlow.API.Features.Products.GetProducts;

public sealed record GetProductsRequest(
    int PageNumber = 1,
    int PageSize = 25,
    string? SearchName = null
    );
