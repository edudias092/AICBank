using AICBank.API.Extensions;
using AICBank.API.Middlewares;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.AddEnvironmentVariables();
builder.Services.AddDefaultConfigurations(builder.Configuration);
builder.Services.AddInternalServices();
builder.Services.AddAuthenticationConfigurations(builder.Configuration);

Log.Information("Starting web server...");
var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
app.UseMiddleware<CustomJwtAuthorizationMiddleware>();
app.MapControllers();
app.UseStaticFiles();

app.Run();
