using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapPost("/create-blog", ([FromBody] PostCreateDTO post_C_DTO) =>
{ //TO DO: add validations later
    PostService handler = new();
    Post post = handler.CreatePost(post_C_DTO);
    PostStore.Posts.Add(post);
    return Results.Created($"/{post.CustomUrl}", post);
})
.WithName("CreateBlog")
.Accepts<PostCreateDTO>("application/json")
.Produces<Post>(201)
.ProducesValidationProblem(); //for when i add the validations

app.UseHttpsRedirection();


app.Run();

