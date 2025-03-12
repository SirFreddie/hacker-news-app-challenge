using Microsoft.Extensions.Caching.Memory;

public class HackerNewsService : IHackerNewsService
{
    private readonly string _baseUrl = "https://hacker-news.firebaseio.com/v0";
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private const string CacheKey = "NewestStories";

    public HackerNewsService(HttpClient httpClient, IMemoryCache cache)
    {
        _httpClient = httpClient;
        _cache = cache;
    }

    public async Task<List<NewsStory>> GetNewestStoriesAsync(int page, int pageSize, string? searchQuery)
    {
        if (!_cache.TryGetValue(CacheKey, out List<NewsStory>? cachedStories))
        {
            var storyIds = await _httpClient.GetFromJsonAsync<int[]>($"{_baseUrl}/newstories.json");
            var stories = new List<NewsStory>();

            if (storyIds != null)
            {
                foreach (var id in storyIds.Take(20)) 
                {
                    var story = await _httpClient.GetFromJsonAsync<NewsStory>($"{_baseUrl}/item/{id}.json");

                    if (story != null && !string.IsNullOrEmpty(story.Url))
                    {
                        stories.Add(story);
                    }
                }
            }

            _cache.Set(CacheKey, stories, TimeSpan.FromMinutes(5));
            cachedStories = stories;
        }

        if (!string.IsNullOrWhiteSpace(searchQuery))
        {
            cachedStories = cachedStories
                .Where(s => s.Title.Contains(searchQuery, StringComparison.OrdinalIgnoreCase))
                .ToList();
        }

        return cachedStories
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();
    }
}
