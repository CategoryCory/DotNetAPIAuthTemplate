#nullable disable

using JwtAuthTemplate.Data;
using JwtAuthTemplate.Data.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace JwtAuthTemplate.Extensions;

public static class IdentityServiceExtensions
{
    public static IServiceCollection ConfigureIdentity(this IServiceCollection services, IConfiguration config)
    {
        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            options.User.RequireUniqueEmail = true;
        })
            .AddEntityFrameworkStores<ApplicationDbContext>()
            .AddDefaultTokenProviders();

        var jwtSecret = config["JwtSettings:SecurityKey"];
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret));

        services.AddAuthentication(authOptions =>
        {
            authOptions.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            authOptions.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
            .AddJwtBearer(bearerOptions =>
            {
                bearerOptions.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = config["JwtSettings:ValidIssuer"],
                    ValidAudience = config["JwtSettings:ValidAudience"],
                    IssuerSigningKey = key,
                    ClockSkew = TimeSpan.Zero,
                };

                bearerOptions.SaveToken = true;
                bearerOptions.RequireHttpsMetadata = false;
                bearerOptions.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        context.Token = context.Request.Cookies[config["JwtSettings:CookieName"]];
                        return Task.CompletedTask;
                    }
                };
            })
            .AddCookie(cookieOptions =>
            {
                cookieOptions.Cookie.SameSite = SameSiteMode.Strict;
                cookieOptions.Cookie.SecurePolicy = CookieSecurePolicy.Always;
                cookieOptions.Cookie.IsEssential = true;
            });

        return services;
    }
}
