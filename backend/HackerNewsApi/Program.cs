using HackerNewsApi.Services;

var MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy =>
                      {
                          policy.WithOrigins("http://localhost:4200",
                                              "http://www.example.com");
                      });
});

builder.Services.AddControllers();

builder.Services.AddOpenApi();
builder.Services.AddMemoryCache();
builder.Services.AddHttpClient();

builder.Services.AddScoped<IHackerNewsService, HackerNewsService>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();

    app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/openapi/v1.json", "My Hacker News API V1");
});
}

app.UseHttpsRedirection();

app.UseCors(MyAllowSpecificOrigins);

app.UseAuthorization();

app.MapControllers();

app.Run();
