using AuditFlow.API.Application.Abstractions.Messaging;
using AuditFlow.API.Domain.Entities;
using AuditFlow.API.Features.Products.CreateProduct;
using AuditFlow.API.Features.Products.Shared;

using FluentResults;

internal sealed class CreateProductCommandHandler : ICommandHandler<CreateProductCommand, CreateUpdateProductResponse>
{
  private readonly ILogger<CreateProductCommandHandler> _logger;

  public CreateProductCommandHandler(ILogger<CreateProductCommandHandler> logger)
  {
    _logger = logger;
  }

  public Task<Result<CreateUpdateProductResponse>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
  {
    var product = CreateProduct(request);

    _logger.LogInformation("Created product with Id: {ProductId}, Name: {ProductName}, Price: {ProductPrice}",
        product.Id.Value, product.Name, product.Price);

    var response = new CreateUpdateProductResponse(
        product.Id.Value,
        product.Name,
        product.Price,
        product.Description,
        product.ImageUrl,
        product.Category,
        product.Size
    );

    return Task.FromResult(Result.Ok(response));
  }

  private static Product CreateProduct(CreateProductCommand createProductCommand)
  {
    return Product.Create(
        createProductCommand.Request.Name,
        createProductCommand.Request.Price,
        createProductCommand.Request.Description,
        createProductCommand.Request.ImageUrl,
        createProductCommand.Request.Category,
        createProductCommand.Request.Size
    );
  }
}
