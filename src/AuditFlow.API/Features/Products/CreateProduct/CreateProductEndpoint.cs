using AuditFlow.API.Features.Products.Shared;
using AuditFlow.API.Shared;
using AuditFlow.API.Shared.Endpoints;

using MediatR;

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using Swashbuckle.AspNetCore.Annotations;

namespace AuditFlow.API.Features.Products.CreateProduct;

[Route(ApiRoute.ProductRoute)]
public sealed class CreateProductEndpoint : EndpointBase
{

  [HttpPost("")]
  [SwaggerOperation(Tags = [nameof(Features)])]
  [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(CreateUpdateProductResponse))]
  [ProducesResponseType(StatusCodes.Status400BadRequest, Type = typeof(ProblemDetails))]
  [AllowAnonymous]
  public async Task<IActionResult> Handle(CreateUpdateProductRequest createProductRequest, [FromServices] IMediator mediator)
  {
    var command = new CreateProductCommand(createProductRequest);
    var commandResult =
      await mediator.Send(command);

    return HandlerResult<CreateUpdateProductResponse>(commandResult, System.Net.HttpStatusCode.Created);
  }

}
