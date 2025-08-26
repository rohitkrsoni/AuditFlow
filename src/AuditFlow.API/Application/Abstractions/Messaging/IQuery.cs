using FluentResults;
using MediatR;

namespace AuditFlow.API.Application.Abstractions.Messaging;

public interface IQuery<TResponse> : IRequest<Result<TResponse>>
{ }
