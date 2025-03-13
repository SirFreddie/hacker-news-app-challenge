using Microsoft.Extensions.Caching.Memory;

namespace HackerNewsApi.Services;

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

    public async Task<PaginatedResponse<NewsStory>> GetNewestStoriesAsync(int page, int pageSize, string? searchQuery)
    {
        // Fetch story IDs (always up-to-date)
        var storyIds = await _httpClient.GetFromJsonAsync<int[]>($"{_baseUrl}/newstories.json");
        if (storyIds == null || storyIds.Length == 0)
        {
            return new PaginatedResponse<NewsStory>(new List<NewsStory>(), 0, page, pageSize);
        }

        var cachedStories = _cache.Get<List<NewsStory>>(CacheKey) ?? new List<NewsStory>();

        // Determine the range of stories to fetch for this page
        var skip = (page - 1) * pageSize;
        var idsToFetch = storyIds.Skip(skip).Take(pageSize).ToList();

        // Check if the page is already cached
        var cachedIds = cachedStories.Select(s => s.Id).ToHashSet();
        var missingIds = idsToFetch.Where(id => !cachedIds.Contains(id)).ToList();

        // Fetch only the missing stories
        var newStories = new List<NewsStory>();
        foreach (var id in missingIds)
        {
            var story = await _httpClient.GetFromJsonAsync<NewsStory>($"{_baseUrl}/item/{id}.json");
            if (story != null && !string.IsNullOrEmpty(story.Url))
            {   
                newStories.Add(story);
            }
        }

        // Merge new stories into cache
        if (newStories.Count > 0)
        {
            cachedStories.AddRange(newStories);
            _cache.Set(CacheKey, cachedStories, TimeSpan.FromMinutes(10)); 
        }

        // Apply search filter
        var filteredStories = string.IsNullOrWhiteSpace(searchQuery)
            ? cachedStories
            : cachedStories.Where(s => s.Title.Contains(searchQuery, StringComparison.OrdinalIgnoreCase)).ToList();

        var totalStories = string.IsNullOrWhiteSpace(searchQuery)
            ? storyIds.Length
            : filteredStories.Count;

        // Get paginated stories from updated cache
        var paginatedStories = filteredStories
            .Skip(skip)
            .Take(pageSize)
            .ToList();

        return new PaginatedResponse<NewsStory>(paginatedStories, totalStories, page, pageSize);
    }
}
