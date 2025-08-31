using FluentResults;

using FluentValidation;

using MediatR;

namespace AuditFlow.API.Application.Behaviours;

internal sealed class ValidationBehaviour<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : ResultBase, new()
{
  private readonly IEnumerable<IValidator<TRequest>> _validators;

  public ValidationBehaviour(IEnumerable<IValidator<TRequest>> validators)
  {
    _validators = validators;
  }

  public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
  {
    var errors = _validators
            .Select(validator => validator.Validate(request))
            .SelectMany(validationResult => validationResult.Errors)
            .Where(failure => failure is not null)
            .Select(failure =>
                new Error(
                        failure.ErrorMessage,
                        new Error(failure.PropertyName)))
            .Distinct()
            .ToArray()
        ;

    if (errors.Length == 0)
    {
      return await next();
    }

    var result = new TResponse();
    result.Reasons.AddRange(errors);

    return result;
  }
}
