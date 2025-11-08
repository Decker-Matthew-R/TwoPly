var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddOpenApi();

// Add CORS so React can call the API
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowReact", policy =>
    {
        policy.WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/openapi/v1.json", "TwoPly API");
    });
}

app.UseHttpsRedirection();

// Enable CORS
app.UseCors("AllowReact");

// API endpoints
app.MapGet("/api/health", () => new { status = "healthy and watching", timestamp = DateTime.UtcNow })
    .WithName("HealthCheck");

app.Run();