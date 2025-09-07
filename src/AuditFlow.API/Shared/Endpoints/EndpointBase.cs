using System.Net;
using System.Net.Mime;

using AuditFlow.API.Application.Common.Errors;

using FluentResults;

using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

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
    /// Handler response for PaginatedListQueryResult
    /// </summary>
    /// <param name="result">The Result object</param>
    /// <typeparam name="TResult">The type of the Result</typeparam>
    /// <typeparam name="TResponse">The type of the Response</typeparam>
    /// <returns></returns>
    //protected IActionResult HandlerResult<TResult, TResponse>(
    //    Result<PaginatedQueryResult<TResult>> result)
    //{
    //  if (result.IsFailed)
    //  {
    //    return Problem(result.Errors);
    //  }

    //  var resultValue = result.Value;
    //  var items = resultValue as IQueryable<TResponse>;

    //  return Ok(PaginatedQueryResult<TResponse>.CreateAsync(items, resultValue.PageNumber, resultValue.PageSize, new CancellationToken()));
    //}

    /// <summary>
    /// Handle Problem details response
    /// </summary>
    /// <param name="errors"></param>
    /// <returns></returns>
    protected IActionResult Problem(IReadOnlyList<IError> errors)
    {
        var isValidationError = errors.Any(e => e.Metadata.Any(s => s.Key.ToLower() == "validation"));

        if (isValidationError)
        {
            return BadRequest(CreateValidationProblemDetails(errors));
        }

        if (errors.Any(e => e.Metadata.ContainsKey(ErrorMetadataType.NotFound)))
        {
            return Problem(
                statusCode: StatusCodes.Status404NotFound,
                title: "Not Found",
                detail: errors[0].Message,
                type: "https://tools.ietf.org/html/rfc9110#section-15.5.5"
            );
        }

        var isAuthenticationError = errors.Any(e => e.Metadata.Any(s => s.Key.ToLower() == "Authentication"));

        if (isAuthenticationError)
        {
            return Unauthorized();
        }

        // ToDo:
        // If none of the above we should return 400
        // But we need to extend this class to support 500 errors
        return BadRequest(CreateValidationProblemDetails(errors));
    }

    static ValidationProblemDetails CreateValidationProblemDetails(IReadOnlyList<IError> errors)
    {
        var modelStateDictionary = new ModelStateDictionary();

        foreach (var error in errors)
        {
            modelStateDictionary.AddModelError(error.Reasons[0].Message, error.Message);
        }

        return new ValidationProblemDetails(modelStateDictionary)
        {
            Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
        };
    }
}
