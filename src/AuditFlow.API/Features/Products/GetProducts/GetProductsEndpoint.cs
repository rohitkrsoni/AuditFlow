using System.Net;

using AuditFlow.API.Features.Products.Shared;
using AuditFlow.API.Shared;
using AuditFlow.API.Shared.Endpoints;

using MediatR;

using Microsoft.AspNetCore.Mvc;

using Swashbuckle.AspNetCore.Annotations;

namespace AuditFlow.API.Features.Products.GetProducts;

[Route(ApiRoute.ProductRoute)]
public sealed class GetProductsEndpoint : EndpointBase
{
    /// <summary>
    /// Retrieve a list of all available products in the system
    /// </summary>
    /// <param name="getProductsRequest"></param>
    /// <returns>
    /// The list of products if successful, returns a with status 200 (OK).
    /// If unauthorized, returns a with status 401 (Unauthorized).
    /// </returns>
    /// <response code="200">Retrieves all products successfully.</response>
    /// <response code="401">Unauthorized. The user is not authorized to perform this action.</response>
    /// <response code="400">Invalid request. The request contains incorrect page number or page size.</response>
    [HttpGet("")]
    [SwaggerOperation(Tags = [nameof(Products)])]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaginatedResponse<CreateUpdateProductResponse>))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
    public async Task<IActionResult> Handle([FromQuery] GetProductsRequest getProductsRequest, [FromServices] IMediator mediator)
    {
        var result = await mediator.Send(new GetProductsQuery(getProductsRequest));
        return HandlerResult(result, HttpStatusCode.OK);
    }
}
