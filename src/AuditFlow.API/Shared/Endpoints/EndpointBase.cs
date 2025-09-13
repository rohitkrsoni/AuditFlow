using System.Net;
using System.Net.Mime;

using AuditFlow.API.Application.Common.Errors;

using FluentResults;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

using Serilog.Filters;

namespace AuditFlow.API.Shared.Endpoints;

[ApiController]
[Produces(MediaTypeNames.Application.Json)]
public abstract class EndpointBase : ControllerBase
{
    /// <summary>
    /// Handle result without response content
    /// </summary>
    /// <param name="result">The Result object</param>
    /// <param name="httpStatusCode"></param>
    /// <returns></returns>
    protected IActionResult HandlerResult(Result result, HttpStatusCode httpStatusCode = HttpStatusCode.OK)
    {
        return result.IsFailed
            ? Problem(result.Errors)
        : httpStatusCode switch
        {
            HttpStatusCode.NoContent => NoContent(),
            HttpStatusCode.Accepted => Accepted(),
            HttpStatusCode.OK => Ok(),
            _ => Ok()
        };
    }

    /// <summary>
    /// Handle result without response content for generic Result type
    /// </summary>
    /// <param name="result">The Result object</param>
    /// <typeparam name="TResult">The type of the Result</typeparam>
    /// <returns></returns>
    protected IActionResult HandlerResult<TResult>(Result<TResult> result, HttpStatusCode httpStatusCode = HttpStatusCode.OK)
      => result.IsFailed ? Problem(result.Errors) : httpStatusCode switch
      {
          HttpStatusCode.NoContent => NoContent(),
          HttpStatusCode.Accepted => Accepted(),
          HttpStatusCode.Created => Created("", result.Value),
          HttpStatusCode.OK => Ok(result.Value),
          _ => Ok()
      };

    /// <summary>
    /// Handle result with a 201 Created response and Location header for generic Result type
    /// </summary>
    /// <param name="result">The Result object</param>
    /// <param name="routeName">The name of the route to generate the Location URI</param>
    /// <param name="routeValues">The route values used to build the URI</param>
    /// <typeparam name="TResult">The type of the Result</typeparam>
    /// <returns></returns>
    protected IActionResult HandlerCreatedAt<TResult>(
        Result<TResult> result,
        string routeName,
        object routeValues)
    {
        return result.IsFailed
            ? Problem(result.Errors)
            : CreatedAtRoute(routeName, routeValues, result.Value);
    }

    /// <summary>
    /// Handle Problem details response
    /// </summary>
    /// <param name="errors"></param>
    /// <returns></returns>
    protected IActionResult Problem(IReadOnlyList<IError> errors)
    {
        var isValidationError = errors.Any(e => e.Metadata.ContainsKey(ErrorMetadataType.Validation));

        if (isValidationError)
        {
            return BadRequest(CreateValidationProblemDetails(errors));
        }

        if (errors.Any(e => e.Metadata.ContainsKey(ErrorMetadataType.NotFound)))
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: ErrorMetadata.NotFound[ErrorMetadataType.NotFound].ToString(),
                detail: errors[0].Message,
                type: "https://tools.ietf.org/html/rfc9110#section-15.5.5"
            );
        }

        // ToDo:
        // If none of the above we should return 400
        // But we need to extend this class to support 500 errors
        return Problem(
            statusCode: StatusCodes.Status400BadRequest,
            title: "Bad Request",
            detail: errors[0].Message,
            type: "https://tools.ietf.org/html/rfc9110#section-15.5.1");
    }

    static ValidationProblemDetails CreateValidationProblemDetails(IReadOnlyList<IError> errors)
    {
        var modelStateDictionary = new ModelStateDictionary();

        foreach (var error in errors)
        {
            var field = error.Metadata.TryGetValue("Field", out var v) && v is string s ? s : "general";
            modelStateDictionary.AddModelError(field, error.Message);
        }

        return new ValidationProblemDetails(modelStateDictionary)
        {
            Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
        };
    }
}
