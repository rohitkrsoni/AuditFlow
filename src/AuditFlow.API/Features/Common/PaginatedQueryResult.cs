using Microsoft.EntityFrameworkCore;

namespace AuditFlow.API.Features.Common;

/// <summary>
/// Represents the result of a collection query, including the list of items and the total number of records.
/// This class extends <see cref="List{T}"/> and is used to return both the collection of items and the total
/// number of records from a query, making it useful for pagination and querying large datasets.
/// </summary>
/// <typeparam name="T">The type of the items in the collection.</typeparam>
public class PaginatedQueryResult<T> : List<T>
{
    /// <summary>
    /// Gets the total number of records available in the source collection.
    /// This value represents the total count of items in the database or source,
    /// and is not limited by any pagination applied to the query.
    /// </summary>
    /// <value>The total number of records.</value>
    public int TotalRecords { get; }

    /// <summary>
    /// Gets the current page number of the results.
    /// </summary>
    /// <value>The current page number.</value>
    public int PageNumber { get; }

    /// <summary>
    /// Gets the number of items per page.
    /// </summary>
    /// <value>The number of items per page.</value>
    public int PageSize { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="PaginatedQueryResult{T}"/> class.
    /// </summary>
    /// <param name="items">The collection of items for the current page.</param>
    /// <param name="pageNumber">The current page number.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="totalRecords">The total number of records in the source collection.</param>
    private PaginatedQueryResult(IEnumerable<T> items, int pageNumber, int pageSize, int totalRecords)
    {
        PageSize = pageSize;
        PageNumber = pageNumber;
        TotalRecords = totalRecords;

        AddRange(items);
    }

    /// <summary>
    /// Creates a new instance of <see cref="PaginatedQueryResult{T}"/> asynchronously from a source query.
    /// </summary>
    /// <param name="source">The queryable data source.</param>
    /// <param name="pageNumber">The current page number.</param>
    /// <param name="pageSize">The number of items per page.</param>
    /// <param name="cancellationToken">A token to observe for cancellation requests.</param>
    /// <returns>A task representing the asynchronous operation. The task result is the <see cref="PaginatedQueryResult{T}"/>.</returns>
    /// <exception cref="ArgumentNullException">Thrown when the source query is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the pageNumber or pageSize is less than or equal to 0.</exception>
    public static async Task<PaginatedQueryResult<T>> CreateAsync(
        IQueryable<T> source,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageNumber, nameof(pageNumber));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(pageSize, nameof(pageSize));

        var count = await source.CountAsync(cancellationToken);

        var items = await source
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PaginatedQueryResult<T>(items, pageNumber: pageNumber, pageSize: pageSize, totalRecords: count);
    }
}
