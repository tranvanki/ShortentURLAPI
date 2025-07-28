using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ShortenURLService.Data;
using ShortenURLService.Services;
using StackExchange.Redis;

namespace ShortenURLService
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            string redisConnection = builder.Configuration.GetConnectionString("Redis") ?? throw new InvalidOperationException("Connection string 'RedisConnection' not found."); 
            builder.Services.AddDbContext<ShortenURLServiceContext>(options =>
              options.UseSqlServer(builder.Configuration.GetConnectionString("ShortenURLServiceContext") 
                        ?? throw new InvalidOperationException("Connection string 'ShortenURLServiceContext' not found.")));

            // Fix the ConfigurationOptions setup:
            var redisOptions = ConfigurationOptions.Parse(redisConnection);
            redisOptions.EndPoints.Clear();
            redisOptions.EndPoints.Add("positive-bengal-12541.upstash.io", 6379);
            redisOptions.Password = "ATD9AAIjcDEwMGQ2ZDI4ZDFkN2I0ODkwOTc5Y" +
                "zc3YmEwY2I5OTgyZHAxMA"; 
            redisOptions.AbortOnConnectFail = false;
            redisOptions.Ssl = true;
            redisOptions.SslProtocols = System.Security.Authentication.SslProtocols.Tls12;
            redisOptions.ConnectTimeout = 10000;
            redisOptions.SyncTimeout = 10000;

            // Add CORS policy
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            // Add services to the container.
            builder.Services.AddControllers();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            try
            {
                var multiplexer = ConnectionMultiplexer.Connect(redisOptions);
                builder.Services.AddSingleton<IConnectionMultiplexer>(sp => multiplexer);
                builder.Services.AddSingleton<IRedisCacheService, RedisCacheService>();
            }
            catch (Exception ex)
            {
                // Log the error
                Console.WriteLine($"Redis connection failed: {ex.Message}");
                
                // Add a mock/fallback implementation
                builder.Services.AddSingleton<IRedisCacheService>();
            }

            builder.Services.AddHttpClient();
            builder.Services.AddScoped<GenerateShortenURL>();
            
            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            // Use CORS policy
            app.UseCors("AllowAll");

            app.UseHttpsRedirection();      
            app.UseAuthorization();
            app.MapControllers();

            app.Run();
        }
    }
}
