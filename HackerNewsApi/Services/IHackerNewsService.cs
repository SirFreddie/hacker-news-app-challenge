public interface IHackerNewsService
{
    Task<List<NewsStory>> GetNewestStoriesAsync(int page, int pageSize, string? searchQuery);
}
