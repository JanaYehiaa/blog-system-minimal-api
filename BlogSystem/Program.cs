using Microsoft.AspNetCore.Mvc;
using System.Text.Json.Serialization;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddScoped<PostService>();
builder.Services.AddAuthentication().AddJwtBearer();

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "Your API", Version = "v1" });

    options.AddSecurityDefinition("Bearer", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Description = "Enter your JWT token in the format: Bearer {your token}"
    });

    options.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            new string[] {}
        }
    });
});

builder.Services.Configure<Microsoft.AspNetCore.Http.Json.JsonOptions>(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddAuthorization(options =>
{
    JwtPolicies.Register(options);
});

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.MapPost("/posts", ([FromBody] PostCreateDTO post_C_DTO, PostService postService, ClaimsPrincipal user) =>
{
    var username = user.FindFirst("username")?.Value;
    var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? user.FindFirst("sub")?.Value;
    var roleStr = user.FindFirst(ClaimTypes.Role)?.Value;

    if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(roleStr) || string.IsNullOrWhiteSpace(username))
    {
        return Results.Unauthorized();
    }

    if (PostStore.Posts.Any(p => p.CustomUrl == post_C_DTO.CustomUrl))
    {
    return Results.Conflict("Custom URL already exists.");
    }

    Post post = postService.CreatePost(post_C_DTO, username, userId);
    PostStore.Posts.Add(post);
    return Results.Created($"/{post.CustomUrl}", post);
})
.RequireAuthorization(JwtPolicies.CanCreate)
.WithName("CreatePost")
.Accepts<PostCreateDTO>("application/json")
.Produces<Post>(201)
.Produces(409)
.Produces(400)
.Produces(401); 

app.MapPost("/posts/{customUrl}/publish", (string customUrl, PostService postService, [FromQuery] DateTime? publishAt) =>
{
    var post = postService.publishPost(customUrl, publishAt);
    return post is not null ? Results.Ok(post) : Results.NotFound();
})
.RequireAuthorization(JwtPolicies.CanPublish)
.WithName("PublishPost")
.Produces<Post>(201)
.Produces(404);


app.MapGet("/posts/{customUrl}", (string customUrl, PostService postService) =>
{
    var dto = postService.GetPostViewDTOByCustomUrl(customUrl);
    return dto is not null ? Results.Ok(dto) : Results.NotFound();
})
.WithName("GetPostByCustomUrl")
.Produces<PostViewDTO>(200)
.Produces(404);

app.MapPut("/posts/{customUrl}", (string customUrl, PostUpdateDTO dto, PostService postService, ClaimsPrincipal user) =>
{
    var username = user.FindFirst("username")?.Value;
    var role = user.FindFirst(ClaimTypes.Role)?.Value;

    var existingPost = PostStore.Posts.FirstOrDefault(p => p.CustomUrl == customUrl);
    if (existingPost is null)
        return Results.NotFound();

    if (role == UserRole.Author.ToString() && existingPost.AuthorUsername != username)
        return Results.Forbid();

    var updatedPost = postService.UpdatePost(dto, customUrl);  
    return updatedPost is not null ? Results.Ok(updatedPost) : Results.NotFound();
})
.RequireAuthorization(JwtPolicies.CanEdit)
.WithName("UpdatePost")
.Accepts<PostUpdateDTO>("application/json")
.Produces<Post>(200)
.Produces(404)
.Produces(403);

app.MapDelete("/posts/{customUrl}", (string customUrl, PostService postService, ClaimsPrincipal user) =>
{
    var username = user.FindFirst("username")?.Value;
    var role = user.FindFirst(ClaimTypes.Role)?.Value;

   var existingPost = PostStore.Posts.FirstOrDefault(p => p.CustomUrl == customUrl);
    if (existingPost is null)
    {
        return Results.NotFound();
    }

    if (role == UserRole.Author.ToString() && existingPost.AuthorUsername != username)
    {
        return Results.Forbid();
    }

    bool deleted = postService.DeletePostByCustomUrl(customUrl);
    return deleted ? Results.NoContent() : Results.NotFound();
})
.RequireAuthorization(JwtPolicies.CanDelete)
.WithName("DeletePost")
.Produces(204)
.Produces(404)
.Produces(403);


app.UseHttpsRedirection();


app.Run();

