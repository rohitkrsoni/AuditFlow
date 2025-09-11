using System.Net;

using AuditFlow.API.Domain.Entities;
using AuditFlow.API.Shared;
using AuditFlow.API.Shared.Endpoints;

using MediatR;

using Microsoft.AspNetCore.Mvc;

using Swashbuckle.AspNetCore.Annotations;

namespace AuditFlow.API.Features.Products.DeleteProduct;

[Route(ApiRoute.ProductRoute)]
public sealed class DeleteProductEndpoint : EndpointBase
{
    /// <summary>
    /// Delete a specified Product in the system
    /// </summary>
    /// <param name="id">The Product unique identifier</param>
    /// <returns></returns>
    /// <response code="204">Product deleted successfully.</response>
    /// <response code="400">The request is invalid</response>
    /// <response code="401">The request lacks valid authentication credentials</response>
    /// <response code="404">The Product Id provided is invalid</response>
    [HttpDelete("{id}")]
    [SwaggerOperation(Tags = [nameof(Products)])]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Handle([FromRoute] Guid id, [FromServices] IMediator mediator)
    {
        var commandResult =
            await mediator.Send(new DeleteProductCommand(new ProductId(id)));

        return HandlerResult(commandResult, HttpStatusCode.NoContent);
    }
}
