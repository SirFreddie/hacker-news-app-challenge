public class PaginatedResponse<T>
{
    public List<T> Data { get; set; } = new();
    public int TotalCount { get; set; }
    public int CurrentPage { get; set; }
    public int PageSize { get; set; }

    public PaginatedResponse(List<T> data, int totalCount, int currentPage, int pageSize)
    {
        Data = data;
        TotalCount = totalCount;
        CurrentPage = currentPage;
        PageSize = pageSize;
    }
}
