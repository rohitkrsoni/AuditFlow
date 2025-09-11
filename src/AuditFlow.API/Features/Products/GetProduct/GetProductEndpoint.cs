using System.Net;

using AuditFlow.API.Domain.Entities;
using AuditFlow.API.Features.Products.Shared;
using AuditFlow.API.Shared;
using AuditFlow.API.Shared.Endpoints;

using MediatR;

using Microsoft.AspNetCore.Mvc;

using Swashbuckle.AspNetCore.Annotations;

namespace AuditFlow.API.Features.Products.GetProduct;

[Route(ApiRoute.ProductRoute)]
public sealed class GetProductEndpoint : EndpointBase
{
    /// <summary>
    ///Get a specified product in the system
    /// </summary>
    /// <param name="id">The request object containing the details for the product to be retrieved.</param>
    /// <returns>
    /// specified product. If successful, returns a with status 200 (OK).
    /// If the request is bad, returns a with status 400 (Bad Request).
    /// If unauthorized, returns a with status 401 (Unauthorized).
    /// </returns>
    /// <response code="200">product retrieved successfully.</response>
    /// <response code="400">Invalid request. The request contains incorrect or missing data.</response>
    /// <response code="401">Unauthorized. The user is not authorized to perform this action.</response>
    /// <response code="404">The Product Id provided is invalid</response>
    [HttpGet("{id:guid}", Name = "GetProductById")]
    [SwaggerOperation(Tags = [nameof(Products)])]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CreateUpdateProductResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
    public async Task<IActionResult> Handle([FromRoute] Guid id, [FromServices] IMediator mediator)
    {
        var query = new GetProductQuery(new ProductId(id));
        var result = await mediator.Send(query);
        return HandlerResult<CreateUpdateProductResponse>(result, HttpStatusCode.OK);
    }
}
