using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using System.Text.Json.Serialization;
using System.Security.Claims;
using FluentValidation;
using System.Text;

using System.Text.Json;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddScoped<PostService>();
builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<IValidator<PostCreateDTO>, PostCreateDTOValidator>();
builder.Services.AddScoped<IValidator<PostUpdateDTO>, PostUpdateDTOValidator>();
builder.Services.AddScoped<IValidator<UserRegisterDTO>, UserRegisterDTOValidator>();
builder.Services.AddScoped<IPasswordHelper, PasswordHelper>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddRazorPages();
builder.Services.AddHttpContextAccessor();


builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

})
.AddJwtBearer(options =>
{
    options.IncludeErrorDetails = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = "my blog",
        ValidAudience = "https://localhost:5222/",
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes("this is custom key for practical aspnetcore sample"))
    };

    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var token = context.Request.Cookies["jwt"];
            if (!string.IsNullOrWhiteSpace(token))
            {
                context.Token = token;
            }
            return Task.CompletedTask;
        }
    };

});

builder.Services.AddHttpClient("BlogAPI", client =>
{
    client.BaseAddress = new Uri("http://localhost:5222");
});

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
app.UseStaticFiles();
app.UseRouting();
//app.UseHttpsRedirection();
app.MapStaticAssets();
app.MapRazorPages();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.MapPost("/posts", async (IValidator<PostCreateDTO> _validation, [FromBody] PostCreateDTO post_C_DTO, PostService postService, ClaimsPrincipal user) =>
{
    var validationResult = await _validation.ValidateAsync(post_C_DTO);

    if (!validationResult.IsValid)
    {
        var error = validationResult.Errors.FirstOrDefault();
        return Results.BadRequest(new
        {
            error = error?.PropertyName,
            errorMessage = error?.ErrorMessage
        });
    }

    var username = user.FindFirst("username")?.Value;
    var userId = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? user.FindFirst("sub")?.Value;
    var roleStr = user.FindFirst(ClaimTypes.Role)?.Value;

    if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(roleStr) || string.IsNullOrWhiteSpace(username))
    {
        return Results.Unauthorized();
    }

    if (await FileStorageHandler.CustomUrlExists(post_C_DTO.CustomUrl))
    {
        return Results.Conflict("Custom URL already exists.");
    }

    Post post = await postService.CreatePost(post_C_DTO, username, userId);
    //PostStore.Posts.Add(post);
    return Results.Created($"/{post.CustomUrl}", post);
})
.RequireAuthorization(JwtPolicies.CanCreate)
.WithName("CreatePost")
.Accepts<PostCreateDTO>("application/json")
.Produces<Post>(201)
.Produces(409)
.Produces(400)
.Produces(401);

app.MapPost("/posts/{customUrl}/publish", async (string customUrl, PostService postService, [FromQuery] DateTime? publishAt) =>
{
    var post = await postService.PublishPost(customUrl, publishAt);
    return post is not null ? Results.Ok(post) : Results.NotFound();
})
.RequireAuthorization(JwtPolicies.CanPublish)
.WithName("PublishPost")
.Produces<Post>(201)
.Produces(404);

app.MapPost("/api/users/login", async (HttpContext httpContext, [FromBody] UserLoginDTO userLoginDTO, UserService userService) =>
{
     if (string.IsNullOrWhiteSpace(userLoginDTO.Username) || string.IsNullOrWhiteSpace(userLoginDTO.Password))
    {
        return Results.BadRequest(new { error = "username or password", errorMessage = "Both fields are required." });
    }
    var user = await userService.GetUser(userLoginDTO);
    if (user is null)
    {
        return Results.BadRequest(new { error = "login", errorMessage = "Invalid username or password." });
    }

    string token = JwtGenerator.GenerateJSONWebToken(user);

   httpContext.Response.Cookies.Append("jwt", token, new CookieOptions
    {
        HttpOnly = true,
        Secure = false,
        SameSite = SameSiteMode.Strict,
        Expires = DateTimeOffset.UtcNow.AddHours(2)
    });

    return Results.Ok(new { token });
})
.WithName("Login")
.Produces<string>(200)
.Produces(401);

app.MapPost("/api/users/register", async (HttpContext httpContext, IValidator<UserRegisterDTO> _validation, [FromBody] UserRegisterDTO userRegisterDTO, UserService userService) =>
{
    var validationResult = await _validation.ValidateAsync(userRegisterDTO);

    if (!validationResult.IsValid)
    {
        var error = validationResult.Errors.FirstOrDefault();
        return Results.BadRequest(new
        {
            error = error?.PropertyName,
            errorMessage = error?.ErrorMessage
        });
    }

    var user = await userService.CreateUser(userRegisterDTO);
    if (user is null)
    {
        return Results.BadRequest(new { error = "Username already exists." });
    }
    string token = JwtGenerator.GenerateJSONWebToken(user);

       httpContext.Response.Cookies.Append("jwt", token, new CookieOptions
    {
        HttpOnly = true,
        Secure = false,
        SameSite = SameSiteMode.Strict,
        Expires = DateTimeOffset.UtcNow.AddHours(2)
    });

    return Results.Ok(new { token });
})
.WithName("Register")
.Produces<string>(200)
.Produces(400);

app.MapGet("/posts/{customUrl}", async (string customUrl, PostService postService) =>
{
    var dto = await postService.GetPostViewDTOByCustomUrl(customUrl);
    return dto is not null ? Results.Ok(dto) : Results.NotFound();
})
.WithName("GetPostByCustomUrl")
.Produces<PostViewDTO>(200)
.Produces(404);

app.MapGet("/posts", async (string? search, PostService postService, int page = 1) =>
{
    const int pageSize = 5;
    var result = await postService.GetPaginatedSummaries(page, pageSize, search);
    return Results.Ok(result);
})
.WithName("GetRecentPublishedPosts")
.Produces<PagedResult<PostPreviewDTO>>(200);

app.MapGet("/posts/category/{category}", async (string category, PostService postService, int page = 1) =>
{
    const int pageSize = 5;
    var result = await postService.GetPaginatedPostsByCategory(category, page, pageSize);
    return Results.Ok(result);
})
.WithName("GetPostsByCategory")
.Produces<PagedResult<PostPreviewDTO>>(200);

app.MapGet("/posts/tags", async (string? tagSearch, PostService postService, int page = 1) =>
{
    const int pageSize = 5;
    var result = await postService.GetPaginatedPostsByTagSearch(tagSearch ?? "", page, pageSize);
    return Results.Ok(result);
})
.WithName("GetPostsByTagSearch")
.Produces<PagedResult<PostPreviewDTO>>(200);

app.MapGet("/categories", async (PostService postService) =>
{
    var categories = await postService.GetAllCategories();
    return Results.Ok(categories);
})
.WithName("GetAllCategories")
.Produces<List<string>>(200);


app.MapPut("/posts/{customUrl}", async (IValidator<PostUpdateDTO> _validation, string customUrl, [FromBody] PostUpdateDTO dto, PostService postService, ClaimsPrincipal user) =>
{
    var validationResult = await _validation.ValidateAsync(dto);

    if (!validationResult.IsValid)
    {
        var error = validationResult.Errors.FirstOrDefault();
        return Results.BadRequest(new
        {
            error = error?.PropertyName,
            errorMessage = error?.ErrorMessage
        });
    }

    var username = user.FindFirst("username")?.Value;
    var role = user.FindFirst(ClaimTypes.Role)?.Value;

    var existingPost = await FileStorageHandler.GetPostByCustomUrl(customUrl);
    if (existingPost is null)
        return Results.NotFound();

    if (role == UserRole.Author.ToString() && existingPost.AuthorUsername != username)
        return Results.Forbid();

    var updatedPost = await postService.UpdatePost(dto, customUrl);
    return updatedPost is not null ? Results.Ok(updatedPost) : Results.NotFound();
})
.RequireAuthorization(JwtPolicies.CanEdit)
.WithName("UpdatePost")
.Accepts<PostUpdateDTO>("application/json")
.Produces<Post>(200)
.Produces(404)
.Produces(403);

app.MapDelete("/posts/{customUrl}", async (string customUrl, PostService postService, ClaimsPrincipal user) =>
{
    var username = user.FindFirst("username")?.Value;
    var role = user.FindFirst(ClaimTypes.Role)?.Value;

    var existingPost = await FileStorageHandler.GetPostByCustomUrl(customUrl);
    if (existingPost is null)
    {
        return Results.NotFound();
    }

    if (role == UserRole.Author.ToString() && existingPost.AuthorUsername != username)
    {
        return Results.Forbid();
    }

    bool deleted = await postService.DeletePostByCustomUrl(customUrl);
    return deleted ? Results.NoContent() : Results.NotFound();
})
.RequireAuthorization(JwtPolicies.CanDelete)
.WithName("DeletePost")
.Produces(204)
.Produces(404)
.Produces(403);


app.Use(async (context, next) =>
{
    context.Request.EnableBuffering();
    var reader = new StreamReader(context.Request.Body);
    var body = await reader.ReadToEndAsync();
    context.Request.Body.Position = 0;
    await next.Invoke();
});

app.Run();

