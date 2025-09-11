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
    [HttpGet("")]
    [SwaggerOperation(Tags = [nameof(Products)])]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaginatedResponse<CreateUpdateProductResponse>))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))] // To Be checked what happens when no product returned
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
    public async Task<IActionResult> Handle([FromQuery] GetProductsRequest getProductsRequest, [FromServices] IMediator mediator)
    {
        var result = await mediator.Send(new GetProductsQuery(getProductsRequest));
        return HandlerResult(result, HttpStatusCode.OK);
    }
}
