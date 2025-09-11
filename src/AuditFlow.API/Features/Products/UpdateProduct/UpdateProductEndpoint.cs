using AuditFlow.API.Domain.Entities;
using AuditFlow.API.Features.Products.Shared;
using AuditFlow.API.Shared;
using AuditFlow.API.Shared.Endpoints;

using MediatR;

using Microsoft.AspNetCore.Mvc;

using Swashbuckle.AspNetCore.Annotations;

namespace AuditFlow.API.Features.Products.UpdateProduct;

[Route(ApiRoute.ProductRoute)]
public sealed class UpdateProductEndpoint : EndpointBase
{
    /// <summary>
    /// Updates an existing product.
    /// </summary>
    /// <param name="id">The Product unique identifier.</param>
    /// <param name="updateProductRequest">The request object containing the updated details for the product.</param>
    /// <returns> An IActionResult that contains the result of the product update operation.</returns>
    /// <response code="200">Product updated successfully.</response>
    /// <response code="400">The request contains incorrect or missing data.</response>
    /// <response code="401">The user is not authorized to perform this action.</response>
    /// <response code="404">The Product Id provided is invalid</response>
    [HttpPut("{id:guid}")]
    [SwaggerOperation(Tags = [nameof(Products)])]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CreateUpdateProductResponse))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    public async Task<IActionResult> Handle([FromRoute] Guid id, [FromBody] CreateUpdateProductRequest updateProductRequest, [FromServices] IMediator mediator)
    {
        var command = new UpdateProductCommand(new ProductId(id), updateProductRequest);
        var commandResult = await mediator.Send(command);
        return HandlerResult(commandResult, System.Net.HttpStatusCode.OK);
    }

}
