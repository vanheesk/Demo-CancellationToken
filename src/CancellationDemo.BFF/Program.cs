using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.AspNetCore.Components.Web;
using Yarp.ReverseProxy.Transforms;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Application Insights
builder.Services.AddApplicationInsightsTelemetry();
builder.Services.AddSingleton<ITelemetryInitializer>(sp => new CustomTelemetryInitializer());
builder.Services.AddSingleton<ITelemetryInitializer>(sp => new DependencyTelemetryInitializer());

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
        //transforms.AddResponseTransform(async context =>
        //{
        //    if (context.HttpContext.RequestAborted.IsCancellationRequested)
        //    {
        //        context.HttpContext.Response.StatusCode = 499;
        //        context.HttpContext.Response.Headers.Add("X-Request-Cancelled", "true");
        //    }
        //});
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

public class CustomTelemetryInitializer : ITelemetryInitializer
{
    public void Initialize(ITelemetry telemetry)
    {
        if (telemetry is RequestTelemetry requestTelemetry)
        {
            if (requestTelemetry.ResponseCode == "499"
                || requestTelemetry.ResponseCode == "Canceled")
            {
                requestTelemetry.Success = true;
            }
        }
    }
}

public class DependencyTelemetryInitializer : ITelemetryInitializer
{
    public void Initialize(ITelemetry telemetry)
    {
        if (telemetry is DependencyTelemetry dependencyTelemetry)
        {
            if (dependencyTelemetry.ResultCode == "499" ||
                dependencyTelemetry.ResultCode == "Canceled" ||
                (dependencyTelemetry.Success == false 
                    && dependencyTelemetry.Properties.ContainsKey("handledAt") 
                    && dependencyTelemetry.Properties["handledAt"] == "OperationCanceledException")
               )
            {
                dependencyTelemetry.Success = true;
                dependencyTelemetry.Properties["handledAt"] = "OperationCanceledException";
            }
        }
    }
}