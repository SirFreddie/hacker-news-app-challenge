using HackerNewsApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace HackerNewsApi.Controllers;

[ApiController]
[Route("api/news")]
public class NewsController : ControllerBase
{
    private readonly IHackerNewsService _hackerNewsService;

    public NewsController(IHackerNewsService hackerNewsService)
    {
        _hackerNewsService = hackerNewsService;
    }

    [HttpGet]
    public async Task<IActionResult> GetNews([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? search = null)
    {
        if (page < 1 || pageSize < 1)
        {
            return BadRequest("Page and page size must be greater than 0");
        }

        var stories = await _hackerNewsService.GetNewestStoriesAsync(page, pageSize, search);
        return Ok(stories);
    }
}
