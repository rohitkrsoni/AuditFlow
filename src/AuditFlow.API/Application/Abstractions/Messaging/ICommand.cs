using FluentResults;

using MediatR;

namespace AuditFlow.API.Application.Abstractions.Messaging;

public interface ICommand : IRequest<Result>
{ }

public interface ICommand<TResponse> : IRequest<Result<TResponse>>
{ }
