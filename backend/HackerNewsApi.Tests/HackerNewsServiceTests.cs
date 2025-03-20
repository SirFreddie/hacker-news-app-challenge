using System.Net;
using System.Net.Http;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Caching.Memory;
using Moq;
using Moq.Protected;
using Xunit;
using FluentAssertions;
using HackerNewsApi.Services;

public class HackerNewsServiceTests
{
    private readonly Mock<IHttpClientFactory> _httpClientFactoryMock;
    private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly HackerNewsService _service;
    private const string CacheKey = "NewestStories";
    private readonly string _baseUrl = "https://hacker-news.firebaseio.com/v0";

    public HackerNewsServiceTests()
    {
        _httpMessageHandlerMock = new Mock<HttpMessageHandler>();

        _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
        {
            BaseAddress = new Uri(_baseUrl)
        };

        // Mocking HttpClientFactory to return the mock HttpClient
        _httpClientFactoryMock = new Mock<IHttpClientFactory>();
        _httpClientFactoryMock
            .Setup(factory => factory.CreateClient(It.IsAny<string>()))
            .Returns(_httpClient);

        _cache = new MemoryCache(new MemoryCacheOptions());
        _service = new HackerNewsService(_httpClientFactoryMock.Object, _cache);
    }

    [Fact]
    public async Task Should_ReturnEmptyResponse_WhenApiReturnsNull()
    {
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("null")
            });

        var result = await _service.GetNewestStoriesAsync(1, 10, null);

        result.Data.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Should_ReturnEmptyResponse_WhenApiFails()
    {
        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.IsAny<HttpRequestMessage>(), ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.InternalServerError));

        var result = await _service.GetNewestStoriesAsync(1, 10, null);

        result.Data.Should().BeEmpty();
        result.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Should_FetchMissingStories_AndCacheThem()
    {
        var storyIds = new[] { 1, 2 };
        var newStory1 = new NewsStory { Id = 1, Title = "Story 1", Url = "https://story1.com" };
        var newStory2 = new NewsStory { Id = 2, Title = "Story 2", Url = "https://story2.com" };

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().EndsWith("/newstories.json")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(storyIds))
            });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("/item/1.json")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(newStory1))
            });

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().Contains("/item/2.json")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(newStory2))
            });

        var result = await _service.GetNewestStoriesAsync(1, 10, null);

        result.Data.Should().HaveCount(2);
        result.Data.Should().ContainSingle(s => s.Id == 1 && s.Title == "Story 1");
        result.Data.Should().ContainSingle(s => s.Id == 2 && s.Title == "Story 2");
    }

    [Fact]
    public async Task Should_ReturnCachedStories_WhenAvailable()
    {
        var cachedStories = new List<NewsStory>
        {
            new() { Id = 1, Title = "Cached Story", Url = "https://example.com" }
        };
        _cache.Set(CacheKey, cachedStories, TimeSpan.FromMinutes(10));

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().EndsWith("/newstories.json")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(new int[] { 1 }))
            });

        var result = await _service.GetNewestStoriesAsync(1, 10, null);

        result.Data.Should().HaveCount(1);
        result.Data.First().Title.Should().Be("Cached Story");
    }

    [Fact]
    public async Task Should_FilterStories_BySearchQuery()
    {
        var cachedStories = new List<NewsStory>
        {
            new() { Id = 1, Title = "Angular News", Url = "https://angular.io" },
            new() { Id = 2, Title = "C# Development", Url = "https://dotnet.microsoft.com" }
        };
        _cache.Set("NewestStories", cachedStories, TimeSpan.FromMinutes(10));

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().EndsWith("/newstories.json")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(new int[] { 1, 2 }))
            });

        var result = await _service.GetNewestStoriesAsync(1, 10, "Angular");

        result.Data.Should().HaveCount(1);
        result.Data[0].Title.Should().Contain("Angular");
    }

    [Fact]
    public async Task Should_RespectPagination_Limits()
    {
        var storyIds = new[] { 1, 2, 3, 4, 5 };
        var cachedStories = new List<NewsStory>
        {
            new() { Id = 1, Title = "Story 1", Url = "https://example.com/1" },
            new() { Id = 2, Title = "Story 2", Url = "https://example.com/2" },
            new() { Id = 3, Title = "Story 3", Url = "https://example.com/3" }
        };
        _cache.Set("NewestStories", cachedStories, TimeSpan.FromMinutes(10));

        _httpMessageHandlerMock.Protected()
            .Setup<Task<HttpResponseMessage>>("SendAsync",
                ItExpr.Is<HttpRequestMessage>(req => req.RequestUri.ToString().EndsWith("/newstories.json")),
                ItExpr.IsAny<CancellationToken>())
            .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(JsonSerializer.Serialize(storyIds))
            });

        var result = await _service.GetNewestStoriesAsync(1, 2, null);

        result.Data.Should().HaveCount(2);
        result.Data[0].Title.Should().Be("Story 1");
        result.Data[1].Title.Should().Be("Story 2");
    }
}
