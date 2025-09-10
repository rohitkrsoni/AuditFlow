namespace AuditFlow.API.Shared;

public sealed record PaginatedResponse<T>
{
    public PaginatedResponse(IEnumerable<T> items, int pageIndex, int pageSize, int totalRecords)
    {
        Items = items;
        PageIndex = pageIndex;
        PageSize = pageSize;
        TotalRecords = totalRecords;
    }

    public IEnumerable<T> Items { get; }

    /// <summary>
    /// The page number of the results to be returned
    /// </summary>
    public int PageIndex { get; }

    /// <summary>
    /// The number of results to be returned per page
    /// </summary>
    public int PageSize { get; }

    /// <summary>
    /// The number of records available to be returned
    /// </summary>
    public int TotalRecords { get; }

    /// <summary>
    /// The number of pages available
    /// </summary>
    public int TotalPages => TotalRecords == 0 ? 0 : (TotalRecords + PageSize - 1) / PageSize;
}
