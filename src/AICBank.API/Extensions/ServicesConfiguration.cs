using System.Text;
using System.Text.Json.Serialization;
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

namespace AICBank.API.Extensions;

public static class ServicesConfiguration
{
    public static IServiceCollection AddLogger(this WebApplicationBuilder builder)
    {
        return builder.Services.AddSerilog(config => config
            .WriteTo.GrafanaLoki("http://localhost:3100")
            .ReadFrom.Configuration(builder.Configuration));      
    }

    public static void AddDefaultConfigurations(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("AICBankDbConnString");
        serviceCollection
            .AddDbContext<AICBankDbContext>(x => x.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString)));

        serviceCollection.AddIdentity<IdentityUser, IdentityRole>()
            .AddEntityFrameworkStores<AICBankDbContext>()
            .AddDefaultTokenProviders()
            .AddErrorDescriber<PortugueseIdentityErrorDescriber>();

        serviceCollection.AddHttpContextAccessor();
        serviceCollection.AddHttpClient();
        serviceCollection.AddMemoryCache();
        
        serviceCollection.AddSerilog(cfg => cfg
            .WriteTo.File("logs/log.txt", rollingInterval: RollingInterval.Day));
        
        serviceCollection.AddAutoMapper(typeof(GlobalMappingProfile).Assembly);
        serviceCollection.AddLogging();
        
        // builder.Services.AddSerilog(config => config
//                 .WriteTo.GrafanaLoki("http://localhost:3100")                
//                 .ReadFrom.Configuration(builder.Configuration));


        serviceCollection.AddControllers().AddJsonOptions(opts =>
        {
            opts.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault;
        });

        // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
        serviceCollection.AddEndpointsApiExplorer();
        serviceCollection.AddSwaggerGen();
    }

    public static void AddAuthenticationConfigurations(this IServiceCollection serviceCollection,
        IConfiguration configuration)
    {
        var secretKey = configuration["JWT:SecretKey"] ?? throw new ArgumentException("Key not found.");

        serviceCollection.AddAuthentication(opts => {
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
                    ValidAudience = configuration["JWT:Audience"],
                    ValidIssuer = configuration["JWT:Issuer"],
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
                };
            });
    }

    public static void AddInternalServices(this IServiceCollection servicesCollection)
    {
        servicesCollection.AddScoped<IAuthRepository, AuthRepository>();
        servicesCollection.AddScoped<IAccountUserRepository, AccountUserRepository>();
        servicesCollection.AddScoped<IBankAccountRepository, BankAccountRepository>();
        servicesCollection.AddScoped<IAuthService, AuthService>();
        servicesCollection.AddScoped<IBankAccountService, BankAccountService>();
        servicesCollection.AddScoped<ICelCashClientService, CelCashClientService>();
        servicesCollection.AddScoped<IEmailService, EmailService>();
        servicesCollection.AddScoped<IValidator<AccountUserDTO>, AccountUserDTOValidator>();
    }
}
