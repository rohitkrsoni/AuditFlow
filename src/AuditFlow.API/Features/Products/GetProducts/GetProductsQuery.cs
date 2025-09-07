using AuditFlow.API.Application.Abstractions.Messaging;
using AuditFlow.API.Features.Products.Shared;
using AuditFlow.API.Shared;

using FluentValidation;

namespace AuditFlow.API.Features.Products.GetProducts;

public sealed record GetProductsQuery(GetProductsRequest Request)
    : IQuery<PaginatedResponse<CreateUpdateProductResponse>>;

internal sealed class GetProductsQueryValidator : AbstractValidator<GetProductsQuery>
{
    public GetProductsQueryValidator()
    {
        RuleFor(x => x.Request.PageNumber)
            .GreaterThan(0).WithMessage("PageNumber must be greater than 0.");

        RuleFor(x => x.Request.PageSize)
            .InclusiveBetween(1, 100).WithMessage("PageSize must be between 1 and 100.");

        When(x => !string.IsNullOrWhiteSpace(x.Request.SearchName), () => RuleFor(x => x.Request.SearchName!)
                .MinimumLength(2).WithMessage("SearchName must be at least 2 characters.")
                .MaximumLength(20).WithMessage("SearchName must be at most 20 characters."));
    }
}
