using System.Text.Json;
using Microsoft.Extensions.Caching.Memory;

namespace HackerNewsApi.Services;

public class HackerNewsService : IHackerNewsService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private const string StoriesIdsCacheKey = "StoryIds";
    private const string NewsStoriesCacheKey = "NewestStories";
    private const int CacheDurationMinutes = 10;
    private readonly string _baseUrl = "https://hacker-news.firebaseio.com/v0";

    public HackerNewsService(HttpClient httpClient, IMemoryCache cache)
    {
        _httpClient = httpClient;
        _cache = cache;
    }

    public async Task<PaginatedResponse<NewsStory>> GetNewestStoriesAsync(int page, int pageSize, string? searchQuery)
    {
        var skip = (page - 1) * pageSize;

        var storyIds = await GetLatestStoryIdsAsync();

        if (storyIds.Length == 0)
        {
            return new PaginatedResponse<NewsStory>(new List<NewsStory>(), 0, page, pageSize);
        }

        var cachedStories = _cache.Get<List<NewsStory>>(NewsStoriesCacheKey) ?? new List<NewsStory>();

        // Find missing stories in cache
        var storiesToFetch = storyIds.Skip(skip).Take(pageSize).ToArray();
        var missingStoryIds = storiesToFetch.Except(cachedStories.Select(s => s.Id)).ToArray();

        // Fetch only missing stories from API
        if (missingStoryIds.Length > 0)
        {
            var fetchedStories = await FetchStoriesFromApi(missingStoryIds);

            cachedStories.AddRange(fetchedStories);
            _cache.Set(NewsStoriesCacheKey, cachedStories, TimeSpan.FromMinutes(CacheDurationMinutes));
        }

        var filteredStories = string.IsNullOrWhiteSpace(searchQuery)
            ? cachedStories
            : cachedStories.Where(s => s.Title.Contains(searchQuery, StringComparison.OrdinalIgnoreCase)).ToList();

        var totalStories = string.IsNullOrWhiteSpace(searchQuery)
            ? storyIds.Length
            : filteredStories.Count;

        var paginatedStories = filteredStories.Skip(skip).Take(pageSize).ToList();

        return new PaginatedResponse<NewsStory>(paginatedStories, totalStories, page, pageSize);
    }

    private async Task<int[]> GetLatestStoryIdsAsync()
    {
        if (_cache.TryGetValue(StoriesIdsCacheKey, out int[] cachedIds))
        {
            return cachedIds;
        }

        try
        {
            var storyIds = await _httpClient.GetFromJsonAsync<int[]>($"{_baseUrl}/newstories.json") ?? Array.Empty<int>();

            if (storyIds.Length > 0)
            {
                _cache.Set(StoriesIdsCacheKey, storyIds, TimeSpan.FromMinutes(CacheDurationMinutes));
            }

            return storyIds;
        }
        catch (HttpRequestException)
        {   
            return Array.Empty<int>();
        }
        catch (Exception)
        {
            return Array.Empty<int>();
        }

    }

    private async Task<List<NewsStory>> FetchStoriesFromApi(int[] storyIds)
    {
        var stories = new List<NewsStory>();
        await Parallel.ForEachAsync(storyIds, async (storyId, _) =>
        {
            var story = await _httpClient.GetFromJsonAsync<NewsStory>($"{_baseUrl}/item/{storyId}.json");
            if (story != null) lock (stories) stories.Add(story);
        });

        return stories;
    }
}
