using AuditFlow.API.Domain.Entities;
using AuditFlow.API.Features.Products.Shared;
using AuditFlow.API.Shared;
using AuditFlow.API.Shared.Endpoints;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Swashbuckle.AspNetCore.Annotations;

namespace AuditFlow.API.Features.Products.UpdateProduct;

[Route(ApiRoute.ProductRoute)]
public sealed class UpdateProductEndpoint : EndpointBase
{

  [HttpPut("{id:guid}")]
  [SwaggerOperation(Tags = [nameof(Features)])]
  [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(CreateUpdateProductResponse))]
  [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
  [AllowAnonymous]
  public async Task<IActionResult> Handle(UpdateProductRequest updateProductRequest, [FromServices] IMediator mediator)
  {
    var command = new UpdateProductCommand(new ProductId(updateProductRequest.Id), updateProductRequest.Request);
    var commandResult = await mediator.Send(command);
    return HandlerResult(commandResult, System.Net.HttpStatusCode.Accepted);
  }

}
