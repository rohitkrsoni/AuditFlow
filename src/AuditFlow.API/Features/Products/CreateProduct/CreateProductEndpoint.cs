using AuditFlow.API.Features.Products.Shared;
using AuditFlow.API.Shared;
using AuditFlow.API.Shared.Endpoints;

using MediatR;

using Microsoft.AspNetCore.Mvc;

using Swashbuckle.AspNetCore.Annotations;

namespace AuditFlow.API.Features.Products.CreateProduct;

[Route(ApiRoute.ProductRoute)]
public sealed class CreateProductEndpoint : EndpointBase
{
    /// <summary>
    /// Creates a new product.
    /// </summary>
    /// <param name="createProductRequest">The request object containing the details for the product to be created.</param>
    /// <returns> An IActionResult that contains the result of the product creation operation.</returns>
    /// <response code="201">Product created successfully.</response>
    /// <response code="400">The request contains incorrect or missing data.</response>
    /// <response code="401">The user is not authorized to perform this action.</response>
    [HttpPost("")]
    [SwaggerOperation(Tags = [nameof(Products)])]
    [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(CreateUpdateProductResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ProblemDetails))]
    public async Task<IActionResult> Handle(CreateUpdateProductRequest createProductRequest, [FromServices] IMediator mediator)
    {
        var command = new CreateProductCommand(createProductRequest);
        var commandResult =
          await mediator.Send(command);

        return HandlerCreatedAt(commandResult, "GetProductById", new { id = commandResult.Value.Id });
    }

}
