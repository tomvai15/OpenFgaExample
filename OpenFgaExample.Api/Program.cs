using Microsoft.AspNetCore.Authorization;
using OpenFga.Sdk.Client;
using OpenFgaExample.Api.Authorization;

namespace OpenFgaExample.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddSingleton<OpenFgaClient>(sp =>
        {
            var config = new ClientConfiguration
            {
                ApiUrl = "http://localhost:8080",
                StoreId = "01K6DWYDK8JPST1ZM5S81QQ7VQ"
            };
            return new OpenFgaClient(config);
        });
        
        builder.Services.AddSingleton<IAuthorizationHandler, FgaHandler>();
        builder.Services.AddSingleton<IAuthorizationPolicyProvider, FgaPolicyProvider>();
        
        // Add services to the container.

        builder.Services.AddControllers();
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseHttpsRedirection();

        app.UseAuthorization();


        app.MapControllers();

        app.Run();
    }
}