using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using OpenFga.Sdk.Client;
using OpenFgaExample.Api.Authorization;
using OpenFgaExample.Core;
using OpenFgaExample.Core.Interfaces;
using OpenFgaExample.Core.Repositories;

namespace OpenFgaExample.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Configure JWT authentication
        var jwtSection = builder.Configuration.GetSection("Jwt");
        var jwtKey = jwtSection.GetValue<string>("Key") ?? throw new InvalidOperationException("JWT key not configured");
        var issuer = jwtSection.GetValue<string>("Issuer");
        var audience = jwtSection.GetValue<string>("Audience");

        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.RequireHttpsMetadata = false;
            options.SaveToken = true;
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = !string.IsNullOrEmpty(issuer),
                ValidIssuer = issuer,
                ValidateAudience = !string.IsNullOrEmpty(audience),
                ValidAudience = audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
                ValidateLifetime = true,
            };
        });

        // Register identity provider
        builder.Services.AddSingleton<IIdentityProvider, IdentityProvider>();

        builder.Services.AddSingleton<OpenFgaClient>(sp =>
        {
            var config = new ClientConfiguration
            {
                ApiUrl = "http://localhost:8080",
                StoreId = "01KK7PBHW87779YPE96XG9GS32"
            };
            return new OpenFgaClient(config);
        });

        builder.Services.AddTransient<IAuthorizationChecker, OpenFgaAuthorizationChecker>();
        builder.Services.AddSingleton<IAuthorizationHandler, FgaHandler>();
        builder.Services.AddSingleton<IAuthorizationPolicyProvider, FgaPolicyProvider>();
        builder.Services.AddSingleton<ICheckRequestProvider, CheckRequestProvider>();
        builder.Services.AddSingleton<IProjectRepository, InMemoryProjectRepository>();
        
        // Add services to the container.
        builder.Services.AddHttpContextAccessor();
        // CORS - allow the dev frontend
        builder.Services.AddCors(options =>
        {
            options.AddDefaultPolicy(policy =>
            {
                policy.WithOrigins("http://localhost:5173")
                      .AllowAnyHeader()
                      .AllowAnyMethod()
                      .AllowCredentials();
            });
        });
        builder.Services.AddControllers();
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.OperationFilter<AddHeaderOperationFilter>();
        });

        var app = builder.Build();
        
        app.UseSwagger();
        app.UseSwaggerUI();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();
        app.UseCors();
        
        app.UseAuthentication();
        
        // Middleware to populate/clear the AsyncLocal-backed identity provider per request
        app.Use(async (context, next) =>
        {
            var idProvider = context.RequestServices.GetRequiredService<IIdentityProvider>();
            try
            {
                idProvider.SetFromClaims(context.User);
                await next();
            }
            finally
            {
                idProvider.Clear();
            }
        });
        
        app.UseAuthorization();

        app.MapControllers();

        app.Run();
    }
}