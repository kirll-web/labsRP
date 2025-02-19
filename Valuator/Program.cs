using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using System.Runtime.Intrinsics.X86;
using System.Text;

namespace Valuator;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);



        // Добавляем Redis
        builder.Services.AddSingleton<IConnectionMultiplexer>(provider =>
            ConnectionMultiplexer.Connect("localhost:6379"));

        builder.Services.AddStackExchangeRedisCache(options =>
        {
            options.Configuration = "localhost:6379";
            options.ConfigurationOptions = new ConfigurationOptions {
                EndPoints = { "localhost:6379" },
                Ssl = false // Set this to true if your Redis instance can handle connection using SSL
            };
        });

        // Add services to the container.
        builder.Services.AddRazorPages();

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (!app.Environment.IsDevelopment())
        {
            app.UseExceptionHandler("/Error");
        }
        app.UseStaticFiles();

        app.UseRouting();

        app.UseAuthorization();

        app.MapRazorPages();

        app.Run();
    }
}
