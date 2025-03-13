using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;
using FluentAssertions;
using HackerNewsApi.Services;
using HackerNewsApi.Controllers;

public class NewsControllerTests
{
    private readonly Mock<IHackerNewsService> _mockService;
    private readonly NewsController _controller;

    public NewsControllerTests()
    {
        _mockService = new Mock<IHackerNewsService>();
        _controller = new NewsController(_mockService.Object);
    }

    [Fact]
    public async Task GetNews_ReturnsOkResult_WithNewsStories()
    {
        // Arrange
        var stories = new PaginatedResponse<NewsStory>(
            new List<NewsStory> { new NewsStory { Id = 1, Title = "Test Story", Url = "https://test.com" } },
            1, 1, 10);

        _mockService
            .Setup(service => service.GetNewestStoriesAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
            .ReturnsAsync(stories);

        // Act
        var result = await _controller.GetNews();

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        var response = okResult.Value as PaginatedResponse<NewsStory>;

        response.Should().NotBeNull();
        response!.Data.Should().ContainSingle(story => story.Title == "Test Story");
    }

    [Fact]
    public async Task GetNews_ReturnsBadRequest_WhenPageIsInvalid()
    {
        // Act
        var result = await _controller.GetNews(0, 10, null);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }
}
