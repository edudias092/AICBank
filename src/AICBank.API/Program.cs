using System.Text;
using System.Text.Json.Serialization;
using AICBank.API.Extensions;
using AICBank.API.Middlewares;
using AICBank.Core.DTOs;
using AICBank.Core.Interfaces;
using AICBank.Core.Mapping;
using AICBank.Core.Services;
using AICBank.Core.Validators;
using AICBank.Data.Context;
using AICBank.Data.Repositories;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using Serilog.Sinks.Grafana.Loki;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Configuration.AddEnvironmentVariables();

var connectionString = builder.Configuration.GetConnectionString("AICBankDbConnString");
builder.Services
        .AddDbContext<AICBankDbContext>(x => x.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

builder.Services.AddIdentity<IdentityUser, IdentityRole>()
                .AddEntityFrameworkStores<AICBankDbContext>()
                .AddDefaultTokenProviders()
                .AddErrorDescriber<PortugueseIdentityErrorDescriber>();

builder.Services.AddHttpContextAccessor();
builder.Services.AddHttpClient();

// builder.Services.AddSerilog(config => config
//                 .WriteTo.GrafanaLoki("http://localhost:3100")                
//                 .ReadFrom.Configuration(builder.Configuration));

builder.Services.AddSerilog(cfg => cfg
    .WriteTo.File("log.txt", rollingInterval: RollingInterval.Day));

Log.Information("Starting web server...");

builder.Services.AddLogging();

builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<IAccountUserRepository, AccountUserRepository>();
builder.Services.AddScoped<IBankAccountRepository, BankAccountRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IBankAccountService, BankAccountService>();
builder.Services.AddScoped<ICelCashClientService, CelCashClientService>();
builder.Services.AddScoped<IEmailService, EmailService>();

builder.Services.AddScoped<IValidator<AccountUserDTO>, AccountUserDTOValidator>();

builder.Services.AddAutoMapper(typeof(GlobalMappingProfile).Assembly);

var secretKey = builder.Configuration["JWT:SecretKey"] ?? throw new ArgumentException("Key not found.");

builder.Services.AddAuthentication(opts => {
    opts.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    opts.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options => {
    options.SaveToken = true;
    options.RequireHttpsMetadata = false;
    options.TokenValidationParameters = new TokenValidationParameters{
        ValidateAudience = true,
        ValidateIssuer = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ClockSkew = TimeSpan.Zero,
        ValidAudience = builder.Configuration["JWT:Audience"],
        ValidIssuer = builder.Configuration["JWT:Issuer"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
    };
});

builder.Services.AddControllers().AddJsonOptions(opts =>
{
    // opts.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.Preserve;
    opts.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault;
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// app.UseSerilogRequestLogging();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// app.UseAuthorization();
app.UseCors(x => x.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
app.UseMiddleware<CustomJwtAuthorizationMiddleware>();
// app.UseHttpsRedirection();
app.MapControllers();
app.UseStaticFiles();

app.Run();
