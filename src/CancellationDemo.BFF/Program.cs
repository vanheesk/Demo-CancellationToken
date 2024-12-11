using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Application Insights
builder.Services.AddApplicationInsightsTelemetry();

// Configure YARP
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"))
    .AddTransforms(transforms =>
    {
        transforms.AddRequestTransform(async context =>
        {
            if (context.HttpContext.RequestAborted.IsCancellationRequested)
            {
                context.HttpContext.Response.StatusCode = 499;
                context.HttpContext.Response.Headers.Add("X-Request-Cancelled", "true");
            }
        });
        transforms.AddResponseTransform(async context =>
        {
            if (context.HttpContext.RequestAborted.IsCancellationRequested)
            {
                context.HttpContext.Response.StatusCode = 499;
                context.HttpContext.Response.Headers.Add("X-Request-Cancelled", "true");
            }
        });
    });

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapReverseProxy();
app.MapControllers();

app.Run();