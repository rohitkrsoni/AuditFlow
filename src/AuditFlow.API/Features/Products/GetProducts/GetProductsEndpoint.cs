using System.Net;

using AuditFlow.API.Domain.Entities;
using AuditFlow.API.Features.Products.Shared;
using AuditFlow.API.Shared;
using AuditFlow.API.Shared.Endpoints;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Swashbuckle.AspNetCore.Annotations;

namespace AuditFlow.API.Features.Products.GetProducts;

[Route(ApiRoute.ProductRoute)]
public sealed class GetProductsEndpoint : EndpointBase
{
    [HttpGet("")]
    [SwaggerOperation(Tags = [nameof(Product)])]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CreateUpdateProductResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
    [AllowAnonymous]
    public async Task<IActionResult> Handle([FromQuery] GetProductsRequest getProductsRequest, [FromServices] IMediator mediator)
    {
        var result = await mediator.Send(new GetProductsQuery(getProductsRequest));
        return HandlerResult(result, HttpStatusCode.OK);
    }
}
