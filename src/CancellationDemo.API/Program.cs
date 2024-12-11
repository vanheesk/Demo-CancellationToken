using Microsoft.ApplicationInsights.DependencyCollector;
using Microsoft.ApplicationInsights.AspNetCore.Extensions;
using Microsoft.ApplicationInsights.Extensibility;
using Microsoft.ApplicationInsights.Channel;
using Microsoft.ApplicationInsights.DataContracts;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add Application Insights
builder.Services.AddApplicationInsightsTelemetry();
builder.Services.AddSingleton<ITelemetryInitializer>(sp => new CustomTelemetryInitializer());
builder.Services.AddSingleton<ITelemetryInitializer>(sp => new DependencyTelemetryInitializer());

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

// Custom telemetry processor class
public class CustomTelemetryInitializer : ITelemetryInitializer
{
    public void Initialize(ITelemetry telemetry)
    {
        if (telemetry is RequestTelemetry requestTelemetry)
        {
            if (requestTelemetry.ResponseCode == "499" 
                || requestTelemetry.ResponseCode == "Canceled" )
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
                (dependencyTelemetry.Success == false && dependencyTelemetry.Properties.ContainsKey("handledAt") &&
                 dependencyTelemetry.Properties["handledAt"] == "OperationCanceledException"))
            {
                dependencyTelemetry.Success = true;
                dependencyTelemetry.Properties["handledAt"] = "OperationCanceledException";
            }
        }
    }
}
