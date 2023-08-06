#nullable disable

using JwtAuthTemplate.Configuration;
using JwtAuthTemplate.Data;
using JwtAuthTemplate.Services;
using Microsoft.EntityFrameworkCore;

namespace JwtAuthTemplate.Extensions;

public static class ApplicationServiceExtensions
{
    public static IServiceCollection ConfigureIISIntegration(this IServiceCollection services)
    {
        services.Configure<IISOptions>(options => { });
        return services;
    }

    public static IServiceCollection ConfigureDatabase(this IServiceCollection services, IConfiguration config)
    {
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(config.GetConnectionString("ApplicationConnection"));
        });

        return services;
    }

    public static IServiceCollection ConfigureLocalServices(this IServiceCollection services, IConfiguration config)
    {
        services.AddScoped<ITokenService, TokenService>();

        return services;
    }
}
