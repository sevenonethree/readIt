using System.Reflection;
using Microsoft.Extensions.Options;
using ReadIt.Models.Reddit;
using ReadIt.Repositories;
using ReadIt.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Allow overrides to the appsettings file to ensure that no secrets get checked in accidentally
// Appsettings files should not contain secrets under normal circumstances... 
// These do (ClientSecret) in an effort to show what needs to be configured.
var configuration = new ConfigurationBuilder()
    .AddEnvironmentVariables()
    .AddJsonFile("appsettings.json", optional: false)
    .AddJsonFile($"appsettings.{(Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "").ToLower()}.json", optional: true)
    .AddUserSecrets(Assembly.GetCallingAssembly(), optional: true, reloadOnChange: true)
    .Build();

builder.Configuration.AddConfiguration(configuration);

builder.Services.Configure<RedditOptions>(builder.Configuration.GetSection(RedditOptions.Name));

// Register In-Memory variants of the repositories. When using real persistance we can update that 
// here. We have the added benefit of being able to write different repositories to different persistance
// layers (e.g. - SQL, json, in-memory, etc.)
builder.Services.AddSingleton<IRepository<PostDetails>, InMemoryRepository<PostDetails>>();
builder.Services.AddSingleton<IRepository<RedditUser>, InMemoryRepository<RedditUser>>();
builder.Services.AddSingleton<IBackgroundTaskWorker, RedditPostRetrievalBackgroundTask>();

// Add the ability to allow future expansion of post sources. Right now, this is limited to Reddit
// but the future could also use twitter or facebook... as a post source
// once I can invent a time machine, I could also use MySpace :)
builder.Services.AddSingleton<IPaginatedPostSource<RedditResponse?>, RedditApi>();

builder.Services.AddHttpClient("Reddit");

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app
    .MapGet("/posts", () => { })
    .WithName("GetPosts")
    .WithOpenApi();
app
    .MapGet("/users", () => { })
    .WithName("GetUsers")
    .WithOpenApi();

// var redditApi = app.Services.GetRequiredService<IPaginatedPostSource<RedditResponse>>();
// var redditOptions = app.Services.GetRequiredService<IOptions<RedditOptions>>();
// var postRepository = app.Services.GetRequiredService<IRepository<PostDetails>>();
var redditPostRetrieval = app.Services.GetRequiredService<IBackgroundTaskWorker>();

// This should always be running in the background, so I am not awaiting this.
_ = Task.Run(redditPostRetrieval.Execute);

app.Run();
