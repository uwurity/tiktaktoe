using System;
using System.IO;
using System.Reflection;
using System.Security.Claims;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.StaticWebAssets;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Stl.DependencyInjection;
using Stl.Fusion;
using Stl.Fusion.Authentication;
using Stl.Fusion.Bridge;
using Stl.Fusion.Client;
using Stl.Fusion.EntityFramework;
using Stl.Fusion.Server;
using Stl.IO;
using Stl.Time;
using tiktaktoe.Abstractions;
using tiktaktoe.Server.Services;

namespace tiktaktoe.Server;

public class Startup
{
    private IConfiguration Cfg { get; }
    private IWebHostEnvironment Env { get; }
    private ServerSettings ServerSettings { get; set; } = null!;
    private ILogger Log { get; set; } = NullLogger<Startup>.Instance;

    public Startup(IConfiguration cfg, IWebHostEnvironment environment)
    {
        Cfg = cfg;
        Env = environment;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        // Logging
        services.AddLogging(logging => {
            logging.ClearProviders();
            logging.AddConsole();
            logging.SetMinimumLevel(LogLevel.Information);
            if (Env.IsDevelopment()) {
                logging.AddFilter("Microsoft", LogLevel.Warning);
                logging.AddFilter("Microsoft.AspNetCore.Hosting", LogLevel.Information);
                logging.AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Information);
                logging.AddFilter("Stl.Fusion.Operations", LogLevel.Information);
            }
        });
        
        // Creating Log and ServerSettings as early as possible
        services.AddSettings<ServerSettings>("Server");
        var tmpServices = services.BuildServiceProvider();
        Log = tmpServices.GetRequiredService<ILogger<Startup>>();
        ServerSettings = tmpServices.GetRequiredService<ServerSettings>();
        
        // DbContext & related services
        var appTempDir = FilePath.GetApplicationTempDirectory("", true);
        var dbPath = appTempDir & "App_v011.db";
        services.AddDbContextFactory<AppDbContext>(db => {
            db.UseSqlite($"Data Source={dbPath}");
            if (Env.IsDevelopment())
                db.EnableSensitiveDataLogging();
        });
        services.AddDbContextServices<AppDbContext>(db => {
            db.AddEntityResolver<long, ChatMessage>();
            db.AddOperations(operations => {
                operations.ConfigureOperationLogReader(_ => new() {
                    // We use FileBasedDbOperationLogChangeTracking, so unconditional wake up period
                    // can be arbitrary long - all depends on the reliability of Notifier-Monitor chain.
                    // See what .ToRandom does - most of timeouts in Fusion settings are RandomTimeSpan-s,
                    // but you can provide a normal one too - there is an implicit conversion from it.
                    UnconditionalCheckPeriod = TimeSpan.FromSeconds(Env.IsDevelopment() ? 60 : 5).ToRandom(0.05),
                });
                operations.AddFileBasedOperationLogChangeTracking();
            });
            db.AddAuthentication<long>();
        });

        var fusion = services.AddFusion();
        var fusionClient = fusion.AddRestEaseClient();
        var fusionServer = fusion.AddWebServer();
        services.AddSingleton(new PublisherOptions() { Id = ServerSettings.PublisherId });
        services.AddSingleton(new PresenceReporter.Options() { UpdatePeriod = TimeSpan.FromMinutes(1) });
        
        // TODO: Fusion services
        
        // Data protection
        services.AddScoped(c => c.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContext());
        services.AddDataProtection().PersistKeysToDbContext<AppDbContext>();
        
        // ASP.NET Core authentication providers
        string domain = $"https://{Cfg["Auth0:Domain"]}/";
        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.Authority = domain;
            options.Audience = Cfg["Auth0:ApiIdentifier"];
            options.TokenValidationParameters = new ()
            {
                NameClaimType = ClaimTypes.NameIdentifier
            };
        });
        
        // Web
        services.Configure<ForwardedHeadersOptions>(options => {
            options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
            options.KnownNetworks.Clear();
            options.KnownProxies.Clear();
        });
        services.AddRouting();
        services.AddMvc().AddApplicationPart(Assembly.GetExecutingAssembly());

        // Swagger & debug tools
        services.AddSwaggerGen(c => {
            c.SwaggerDoc("v1", new () {
                Title = "tiktaktoe.Server API", Version = "v1"
            });
        });
    }
    
    public void Configure(IApplicationBuilder app, ILogger<Startup> log)
    {
        if (ServerSettings.AssumeHttps) {
            Log.LogInformation("AssumeHttps on");
            app.Use((context, next) => {
                context.Request.Scheme = "https";
                return next();
            });
        }

        if (Env.IsDevelopment()) {
            app.UseDeveloperExceptionPage();
        }
        else {
            app.UseExceptionHandler("/Error");
            app.UseHsts();
        }
        app.UseHttpsRedirection();
        app.UseForwardedHeaders(new () {
            ForwardedHeaders = ForwardedHeaders.XForwardedProto
        });

        app.UseWebSockets(new () {
            KeepAliveInterval = TimeSpan.FromSeconds(30),
        });
        app.UseFusionSession();

        // Static + Swagger
        app.UseSwagger();
        app.UseSwaggerUI(c => {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
        });

        // API controllers
        app.UseRouting();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseEndpoints(endpoints => {
            endpoints.MapFusionWebSocketServer();
            endpoints.MapControllers();
        });
    }
}