using Serilog;
using Serilog.Sinks.Grafana.Loki;

namespace AICBank.API.Extensions;

public static class ServicesConfiguration
{
    public static IServiceCollection AddLogger(this WebApplicationBuilder builder)
    {
        return builder.Services.AddSerilog(config => config
            .WriteTo.GrafanaLoki("http://localhost:3100")
            .ReadFrom.Configuration(builder.Configuration));      
    }
}
