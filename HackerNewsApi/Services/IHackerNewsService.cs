public interface IHackerNewsService
{
    Task<PaginatedResponse<NewsStory>> GetNewestStoriesAsync(int page, int pageSize, string? searchQuery);
}
