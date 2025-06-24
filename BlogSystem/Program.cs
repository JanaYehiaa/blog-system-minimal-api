using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapPost("/create-blog", ([FromBody] PostCreateDTO post_C_DTO) =>
{ //TO DO: add validations later
    PostService handler = new();
    Post post = handler.CreatePost(post_C_DTO, "admin");
    PostStore.Posts.Add(post);
    return Results.Created($"/{post.CustomUrl}", post);
})
.WithName("CreateBlog")
.Accepts<PostCreateDTO>("application/json")
.Produces<Post>(201)
.ProducesValidationProblem(); //for when i add the validations


app.MapGet("/{customUrl}", (string customUrl) =>
{
    PostService handler = new();
    var dto = handler.GetPostByCustomUrl(customUrl);
    return dto is not null ? Results.Ok(dto) : Results.NotFound();
})
.WithName("GetPostByCustomUrl")
.Produces<PostViewDTO>(200)
.Produces(404);

app.UseHttpsRedirection();


app.Run();

