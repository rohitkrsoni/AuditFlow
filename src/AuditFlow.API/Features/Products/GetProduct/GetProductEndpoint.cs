using System.Net;

using AuditFlow.API.Domain.Entities;
using AuditFlow.API.Features.Products.Shared;
using AuditFlow.API.Shared;
using AuditFlow.API.Shared.Endpoints;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Swashbuckle.AspNetCore.Annotations;

namespace AuditFlow.API.Features.Products.GetProduct;

[Route(ApiRoute.ProductRoute)]
public sealed class GetProductEndpoint : EndpointBase
{
    [HttpGet("{id:guid}", Name = "GetProductById")]
    [SwaggerOperation(Tags = [nameof(Products)])]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(CreateUpdateProductResponse))]
    [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ProblemDetails))]
    [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
    [AllowAnonymous]
    public async Task<IActionResult> Handle([FromRoute] Guid id, [FromServices] IMediator mediator)
    {
        var query = new GetProductQuery(new ProductId(id));
        var result = await mediator.Send(query);
        return HandlerResult<CreateUpdateProductResponse>(result, HttpStatusCode.OK);
    }
}
