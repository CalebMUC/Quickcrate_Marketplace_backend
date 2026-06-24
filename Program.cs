using Microsoft.EntityFrameworkCore;
//using Minimart_Api.Data;
using Minimart_Api.Repositories;
using Minimart_Api.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.OpenApi.Models;
using Microsoft.Extensions.Logging;
using Serilog;
using Minimart_Api.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Authentication_and_Authorization_Api.Core;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.Extensions.FileProviders;
using Minimart_Api.Mappings;
using Minimart_Api.Services.RabbitMQ;
using Minimart_Api.Services.NotificationService;
using OpenSearch.Client;
using OpenSearch.Net;
using Microsoft.Extensions.Options;
using Minimart_Api.Services.OpenSearchService;
using Elasticsearch.Net;
using Elasticsearch.Net.Aws;
using Amazon.Runtime;
using Amazon;
using Amazon.Extensions.NETCore.Setup;
using Minimart_Api.Services.SystemSecurity;
using Minimart_Api.Repositories.SystemSecurityRepo;
//using Minimart_Api.Services.SystemMerchantService;
//using Minimart_Api.Repositories.SystemMerchantsRepository;
using Minimart_Api.Services.ProductService;
using Minimart_Api.Repositories.ProductRepository;
using Minimart_Api.Services.CategoriesService;
using Minimart_Api.Repositories.CategoriesRepository;
using Minimart_Api.Services.SignalR;
using Minimart_Api.DTOS.Notification;
using Minimart_Api.DTOS.Payments;
using Minimart_Api.DTOS.Authorization;
using Minimart_Api.Repositories.Authorization;
//using Minimart_Api.Repositories.Merchants;
using Minimart_Api.Repositories.Order;
using Minimart_Api.Repositories.Reports;
using Minimart_Api.Repositories.Search;
using Minimart_Api.Services.OrderService;
using Minimart_Api.Services.ReportService;
using Minimart_Api.Services.SearchService;
using Minimart_Api.Services.OrderService.OrderService;
using Minimart_Api.Services.SearchService.SearchService;
using Minimart_Api.Services.ReportService.ReportService;
using Minimart_Api.Data;
using StackExchange.Redis;
using Minimart_Api.Services.EmailServices;
using Minimart_Api.Services.Features;
using Minimart_Api.Repositories.Features;
using Minimart_Api.Services.Cart;
using Minimart_Api.Repositories.Cart;
using Minimart_Api.Services.SimilarProducts;
using Minimart_Api.Services.Recommedation;
using Minimart_Api.Repositories.Recommendation;
using Minimart_Api.Services.Deliveries;
using Minimart_Api.Repositories.Deliveries;
using Minimart_Api.Services.Address;
using Minimart_Api.Repositories.AddressesRepo;
using Microsoft.AspNetCore.HttpOverrides;
using Minimart_Api.Services.Mpesa;
using Minimart_Api.Repositories.Mpesa;
using Minimart_Api.Services.Identity;
using Minimart_Api.Services.Category;
using Minimart_Api.DTOS.Configuration;
using Minimart_Api.Repositories.Category;
using Minimart_Api.Services.Merchant;
using Minimart_Api.Repositories.Merchant;
using Minimart_Api.Services.CurrentUserServices;
using Minimart_Api.Services.PasswordGenerator;
using Minimart_Api.Services.Dashboard;
using Minimart_Api.Repositories.Dashboard;
using Minimart_Api.Services.PaymentMethods;
using Minimart_Api.Services.Payouts;
using Minimart_Api.DTOS.AWS;


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add AutoMapper
builder.Services.AddAutoMapper(typeof(CategoryMappingProfile), typeof(ProductMappingProfile));

builder.Services.AddScoped<IMyService, MyService>();
builder.Services.AddScoped<IRepository, MyRepository>();
builder.Services.AddScoped<IOrderService, OrderServices>();
builder.Services.AddScoped<IorderRepository, OrderRepository>();

// Add Order Validation Service
builder.Services.AddScoped<Minimart_Api.Services.OrderService.IOrderValidationService, Minimart_Api.Services.OrderService.OrderValidationService>();

// Add database-based Category service and repository - FIXED REGISTRATION
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ICategoryRepo, CategoryRepo>();

builder.Services.AddScoped<ISearchService, SearchServices>();
builder.Services.AddScoped<ISearchRepo,SearchRepo>();

//builder.Services.AddScoped<IMerchantService, MerchantService>();
//builder.Services.AddScoped<IMerchantRepo, MerchantRepo>();

builder.Services.AddScoped<IReportService, ReportServices>();
builder.Services.AddScoped<IReportRepo, ReportRepo>();

builder.Services.AddScoped<IFeatureService, FeatureService>();
builder.Services.AddScoped<IFeatureRepo, FeatureRepo>();

// Cart Services - Updated to use new implementation
builder.Services.AddScoped<ICartService, CartService>();
builder.Services.AddScoped<ICartRepository, CartRepository>();

// Add legacy cart repository for backward compatibility in CartService
builder.Services.AddScoped<ICartRepo, CartRepo>();

builder.Services.AddScoped<IDeliveryService, DeliveryService>();
builder.Services.AddScoped<IDeliveriesRepo, DeliveriesRepo>();

// Enhanced Address Services with legacy support
builder.Services.AddScoped<IAddressService, AddressServiceNew>();
builder.Services.AddScoped<IAddress, AddressServiceNew>(); // Legacy interface pointing to enhanced service
builder.Services.AddScoped<IAddressRepository, AddressRepositoryNew>();
builder.Services.AddScoped<IAddressRepo, AddressRepositoryNew>(); // Legacy interface pointing to enhanced repository

// Payment Method Services
builder.Services.AddScoped<IPaymentMethodService, PaymentMethodService>();

// Payout Services
builder.Services.AddScoped<IPayoutService, PayoutService>();

builder.Services.AddScoped<ISimilarProductsService, SimilarProductsService>();

builder.Services.AddScoped<IAuthentication, AuthenticationService>();
builder.Services.AddScoped<IAuthRepository, AuthRepository>();

builder.Services.AddScoped<IMerchantService, MerchantService>();
builder.Services.AddScoped<IMerchantRepo, MerchantRepo>();

builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<ICurrentUserService,CurrentUserService>();

builder.Services.AddScoped<IPasswordGeneratorService, PasswordServiceGenerator>();

// Add Identity Services
builder.Services.AddScoped<IIdentityService, IdentityService>();

builder.Services.AddScoped<ISystemSecurity, SystemSecurity>();
builder.Services.AddScoped<ISystemSecurityRepo, SystemSecurityRepo>();


builder.Services.AddScoped<IRecomedationService, RecommendationService>();
builder.Services.AddScoped<IRecommendationRepository, RecommendationRepository>();

builder.Services.AddScoped<IProductService, ProductService>();
builder.Services.AddScoped<IProductRepository, ProductRepository>();

// Add SEO Slug Service
builder.Services.AddScoped<Minimart_Api.Services.SlugService.ISlugService, Minimart_Api.Services.SlugService.SlugService>();

//builder.Services.AddScoped<ICategoriesService, CategoriesNewService>();
//builder.Services.AddScoped<ICategoryRepos, CategoryRepos>();

builder.Services.AddScoped<IMpesaService, MpesaService>();
builder.Services.AddScoped<IMpesaRepo, MpesaRepo>();

builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddScoped<IDashboardRepo, DashboardRepo>();

// Add Enhanced Dashboard Services
builder.Services.AddScoped<IEnhancedDashboardService, EnhancedDashboardService>();

builder.Services.AddScoped<IOrderEventPublisher, OrderEventPublisher>();
builder.Services.AddHostedService<OrderEventConsumer>();

builder.Services.AddScoped<INotfication, NotificationService>();

//builder.Services.AddScoped<IOpenSearchService, OpenSearchService>();


builder.Services.AddScoped<CoreLibraries>();
builder.Services.AddScoped<OrderMapper>();

builder.Services.AddScoped<BrevoEmailService>();

builder.Services.AddMemoryCache();
builder.Services.AddResponseCaching();

// Register RabbitMQ connection
builder.Services.AddSingleton<IRabbitMqConnection, RabbitMqConnection>();

// Optional: Add health check
//builder.Services.AddHealthChecks()
//    .AddRabbitMQ(provider =>
//        provider.GetRequiredService<IRabbitMqConnection>().Connection);



//configure Serilog

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .WriteTo.File("Logs/app-.log", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Host.UseSerilog();

//configur Logging
builder.Logging.ClearProviders();
builder.Logging.AddConsole();
builder.Logging.AddDebug(); 
builder.Services.AddDbContext<MinimartDBContext>(options =>
{
    // options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"))
    //.LogTo(message => Log.Information(message), Microsoft.Extensions.Logging.LogLevel.Information) // Log to Serilog
    //.EnableSensitiveDataLogging() // Enable logging of sensitive data (like parameters)

    //options.UseNpgsql(builder.Configuration.GetConnectionString("PostgressConnection"))
    //.LogTo(error => Log.Error(error))
    //.EnableSensitiveDataLogging();

    options.UseNpgsql(
    //builder.Configuration.GetConnectionString("PostgressConnection"),
    builder.Configuration.GetConnectionString("DefaultConnection"),
    npgsqlOptions =>
    {
        npgsqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorCodesToAdd: null
        );
    })
    .LogTo(error => Log.Error(error))
    .EnableSensitiveDataLogging();

},
ServiceLifetime.Scoped); // Scoped lifetime for the DbContext

// Configure ASP.NET Core Identity
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    // Password settings
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequiredLength = 6;
    options.Password.RequiredUniqueChars = 1;

    // Lockout settings
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.Lockout.AllowedForNewUsers = true;

    // User settings
    options.User.AllowedUserNameCharacters = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";
    options.User.RequireUniqueEmail = true;

    // Email confirmation settings
    options.SignIn.RequireConfirmedEmail = false; // Set to true if you want email confirmation
    options.SignIn.RequireConfirmedPhoneNumber = false;
})
.AddEntityFrameworkStores<MinimartDBContext>()
.AddDefaultTokenProviders();


builder.Services.AddSingleton<IConnectionMultiplexer>(sp => {
    var logger = sp.GetRequiredService<ILogger<Program>>();
    var env = sp.GetRequiredService<IHostEnvironment>();
    //Get Connection string from environment variable or appsettings
    var redisConnection = Environment.GetEnvironmentVariable("REDISURL") ?? builder.Configuration.GetConnectionString("Redis");

    //check if redis is null or whitespace
    if (string.IsNullOrEmpty(redisConnection)) {
        throw new InvalidOperationException("Redis connection string is missing");
    }
    //parse redis connection string and assign options based on environment
    var options = ConfigurationOptions.Parse(redisConnection);

    //assign options based on environment
    options.AbortOnConnectFail = false;
    options.ConnectTimeout = 10000; // 10 seconds
    options.SyncTimeout = 5000; // 5 seconds

    if (env.IsDevelopment()) {
        options.Ssl = false; // Disable SSL in development
        options.DefaultDatabase = 0; // Use default database in development
    } else {
        options.Ssl = true; // Enable SSL in production
        options.DefaultDatabase = 0; // Use default database in production
    }

    //connect to redis and log connection status
    var connection = ConnectionMultiplexer.Connect(options);

    connection.ConnectionFailed += (sender, args) => {
        logger.LogError(args.Exception, "Redis connection failed to {Endpoint}", args.EndPoint);
    };
    connection.ConnectionRestored += (sender, args) => {
        logger.LogInformation("Redis connection restored to {Endpoint}", args.EndPoint);
    };

    return connection;





});

//Development
//builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
//{
//var logger = sp.GetRequiredService<ILogger<IConnectionMultiplexer>>();

//var redisUrl = Environment.GetEnvironmentVariable("REDIS_URL")
//              ?? builder.Configuration.GetConnectionString("Redis"));

//if (string.IsNullOrWhiteSpace(redisUrl))
//    throw new InvalidOperationException("Redis connection string is missing.");

//logger.LogInformation("Connecting to Redis via URI: {RedisUrl}", redisUrl);

//try
//{
//    // Parse the URL into ConfigurationOptions for better control
//    var config = ConfigurationOptions.Parse(redisUrl);

//    // Recommended settings for Upstash
//    config.AbortOnConnectFail = false; // Retry on failure
//    config.ConnectTimeout = 10000;    // 10 seconds
//    config.SyncTimeout = 5000;        // 5 seconds  
//    config.Ssl = true;                 // Force SSL (Upstash requires it)
//    config.DefaultDatabase = 0;        // Explicitly set DB if needed

//    var connection = ConnectionMultiplexer.Connect(config);

//    connection.ConnectionFailed += (_, args) =>
//        logger.LogError(args.Exception, "Redis connection failed to {Endpoint}", args.EndPoint);

//    connection.ConnectionRestored += (_, args) =>
//        logger.LogInformation("Redis connection restored to {Endpoint}", args.EndPoint);

//    logger.LogInformation("Redis connected successfully.");
//    return connection;
//}
//catch (Exception ex)
//{
//    logger.LogCritical(ex, "Failed to connect to Redis.");
//    throw;
//}
//});

//Production
//builder.Services.Configure<Minimart_Api.DTOS.Configuration.RedisSettings>(
//    builder.Configuration.GetSection("RedisSettings"));

//builder.Services.AddSingleton<IConnectionMultiplexer>(sp =>
//{
//    var logger = sp.GetRequiredService<ILogger<IConnectionMultiplexer>>();
//    var environment = sp.GetRequiredService<IWebHostEnvironment>();
//    var redisSettings = sp.GetRequiredService<IOptions<Minimart_Api.DTOS.Configuration.RedisSettings>>().Value;

//    // Get Redis URL from environment or configuration
//    var redisUrl = Environment.GetEnvironmentVariable("REDIS_URL")
//                   ?? builder.Configuration.GetConnectionString("Redis");

//    if (string.IsNullOrWhiteSpace(redisUrl))
//        throw new InvalidOperationException("Redis connection string is missing.");

//    logger.LogInformation("Environment: {Environment}", environment.EnvironmentName);
//    logger.LogInformation("Connecting to Redis: {RedisUrl}", redisUrl.Contains("@") ? "[PROTECTED]" : redisUrl);

//    try
//    {
//        ConfigurationOptions config;

//        if (environment.IsDevelopment())
//        {
//            // Development Configuration
//            logger.LogInformation("Using Development Redis configuration");
            
//            config = ConfigurationOptions.Parse(redisUrl);
            
//            // Apply development settings from configuration
//            config.AbortOnConnectFail = redisSettings.AbortOnConnectFail;
//            config.ConnectTimeout = redisSettings.ConnectTimeout;
//            config.SyncTimeout = redisSettings.SyncTimeout;
//            config.DefaultDatabase = redisSettings.Database;
//            config.Ssl = redisSettings.UseSSL;
            
//            // Override SSL if cloud Redis is detected in development
//            if (redisUrl.Contains("upstash.io") || redisUrl.Contains("redis.cloud") || redisUrl.Contains("amazonaws.com"))
//            {
//                config.Ssl = true;
//                logger.LogInformation("Cloud Redis detected - enabling SSL for development");
//            }
//        }
//        else
//        {
//            // Production Configuration
//            logger.LogInformation("Using Production Redis configuration");
            
//            if (redisSettings.IsUpstash && !string.IsNullOrEmpty(redisSettings.UpstashEndpoint))
//            {
//                // Upstash Redis configuration with settings from appsettings
//                config = new ConfigurationOptions
//                {
//                    EndPoints = { redisSettings.UpstashEndpoint },
                    
//                    // Extract password from URL or use direct environment variable
//                    Password = Environment.GetEnvironmentVariable("REDIS_PASSWORD") 
//                              ?? (redisUrl.Contains("@") ? redisUrl.Split('@')[0].Split(':')[2] : null),
                    
//                    Ssl = redisSettings.UseSSL,
//                    AbortOnConnectFail = redisSettings.AbortOnConnectFail,
//                    ConnectTimeout = redisSettings.ConnectTimeout,
//                    SyncTimeout = redisSettings.SyncTimeout,
//                    DefaultDatabase = redisSettings.Database
//                };
                
//                logger.LogInformation("Using Upstash configuration with endpoint: {Endpoint}", redisSettings.UpstashEndpoint);
//            }
//            else
//            {
//                // Generic production Redis configuration
//                config = ConfigurationOptions.Parse(redisUrl);
//                config.Ssl = redisSettings.UseSSL;
//                config.AbortOnConnectFail = redisSettings.AbortOnConnectFail;
//                config.ConnectTimeout = redisSettings.ConnectTimeout;
//                config.SyncTimeout = redisSettings.SyncTimeout;
//                config.DefaultDatabase = redisSettings.Database;
                
//                logger.LogInformation("Using generic Redis configuration");
//            }
//        }

//        var connection = ConnectionMultiplexer.Connect(config);

//        // Attach event handlers for monitoring
//        connection.ConnectionFailed += (_, e) =>
//            logger.LogError(e.Exception, "Redis connection failed to {Endpoint} in {Environment}", 
//                e?.EndPoint, environment.EnvironmentName);

//        connection.ConnectionRestored += (_, e) =>
//            logger.LogInformation("Redis connection restored to {Endpoint} in {Environment}", 
//                e?.EndPoint, environment.EnvironmentName);

//        logger.LogInformation("Redis connected successfully in {Environment} mode with SSL: {SSL}", 
//            environment.EnvironmentName, config.Ssl);
        
//        return connection;
//    }
//    catch (Exception ex)
//    {
//        logger.LogCritical(ex, "Failed to connect to Redis in {Environment} environment", environment.EnvironmentName);
//        throw;
//    }
//});



// 2. Redis (TLS-enabled)
//builder.Services.AddSingleton<IConnectionMultiplexer>(_ =>
//    ConnectionMultiplexer.Connect(
//        Environment.GetEnvironmentVariable("REDIS_URL") ??
//        builder.Configuration.GetConnectionString("Redis"),
//        options => options.Ssl = true
//    ));

builder.Services.AddScoped(provider =>
{
    var redis = provider.GetRequiredService<IConnectionMultiplexer>();
    return redis.GetDatabase();
});
builder.Services.AddScoped<MpesaSandBox>();

//builder.Services.AddHostedService<SyncProductsToOpenSearch>();


builder.Services.Configure<MpesaSandBox>(builder.Configuration.GetSection("MpesaSandBox"));

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

builder.Services.Configure<CelcomAfrica>(builder.Configuration.GetSection("CelcomAfrica"));

builder.Services.Configure<MpesaGoLive>(builder.Configuration.GetSection("MpesaGoLive"));

builder.Services.Configure<AwsConfig>(builder.Configuration.GetSection("AwsConfig"));



builder.Services.AddSignalR();


// Add JWT authentication
//builder.Services.AddAuthentication(options =>
//{
//    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
//    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
//})
//.AddJwtBearer(options =>
//{
//    var jwtSettings = builder.Configuration.GetSection("Jwt");
//    options.TokenValidationParameters = new TokenValidationParameters
//    {
//        ValidateIssuer = false,
//        ValidateAudience = false,
//        ValidateLifetime = true,
//        ValidateIssuerSigningKey = true,
//        //ValidIssuer = builder.Configuration["Jwt:Issuer"],
//        //ValidAudience = builder.Configuration["Jwt:Audience"],
//        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings["Key"]))
//    };
//});

// Add JWT authentication
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    var jwtSettings = builder.Configuration.GetSection("JwtSettings");

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateIssuerSigningKey = true,
        ValidateLifetime = true,

        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],

        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings["Secret"])
        )
    };
});


// Add authorization policies
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("MyPolicy", policy =>
    {
        policy.RequireAuthenticatedUser();
    });

    // Add role-based policies
    options.AddPolicy("AdminOnly", policy =>
        policy.RequireRole("Admin"));

    options.AddPolicy("MerchantOnly", policy =>
        policy.RequireRole("Merchant"));

    options.AddPolicy("UserOnly", policy =>
        policy.RequireRole("User"));

    options.AddPolicy("AdminOrMerchant", policy =>
        policy.RequireRole("Admin", "Merchant"));
});

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new QueryStringApiVersionReader("api-version");

});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Api",
        Version = "v1"
    });

    // Add JWT Authentication
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement{
    {
        new OpenApiSecurityScheme
        {
            Reference = new OpenApiReference
            {
                Type = ReferenceType.SecurityScheme,
                Id = "Bearer"
            }
        },
        new string[] {}
    }});

});
// Add services to the container.
//builder.Services.AddCors(options =>
//{
//    options.AddPolicy("AllowFrontend", builder =>
//    {
//        //https://minimart-nine.vercel.app
//        //http://localhost:3000
//        builder.WithOrigins("http://localhost:3000")
//               .AllowAnyMethod()
//               .AllowAnyHeader()
//               .AllowCredentials();
//    });
//});


builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});



builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;
    options.KnownNetworks.Clear(); // Optional for proxy environments
    options.KnownProxies.Clear();  // Optional for proxy environments
});



builder.Services.AddHttpClient();


var app = builder.Build();

// Seed roles on startup
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();

    // Ensure roles exist
    string[] roleNames = { "Admin", "Merchant", "User" };
    foreach (var roleName in roleNames)
    {
        var roleExists = await roleManager.RoleExistsAsync(roleName);
        if (!roleExists)
        {
            await roleManager.CreateAsync(new ApplicationRole 
            { 
                Name = roleName, 
                Description = $"{roleName} role" 
            });
        }
    }

    // Create default admin user if needed
    //var adminEmail = "admin@minimart.com";
    //var adminUser = await userManager.FindByEmailAsync(adminEmail);
    //if (adminUser == null)
    //{
    //    adminUser = new ApplicationUser
    //    {
    //        UserName = adminEmail,
    //        Email = adminEmail,
    //        DisplayName = "System Administrator",
    //        EmailConfirmed = true,
    //        CreatedAt = DateTime.UtcNow
    //    };

    //    await userManager.CreateAsync(adminUser, "Admin123!");
    //    await userManager.AddToRoleAsync(adminUser, "Admin");
    //}
}

// Enable forwarded headers middleware BEFORE any URL generation or redirect logic
app.UseForwardedHeaders();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Minimart API v1");
        c.ConfigObject.AdditionalItems["servers"] = new List<Dictionary<string, string>>
        {
            new Dictionary<string, string> { { "url", "/" } }
        };
    });
}
else
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Minimart API v1");
        c.RoutePrefix = "swagger";
        c.ConfigObject.AdditionalItems["servers"] = new List<Dictionary<string, string>>
        {
            new Dictionary<string, string> { { "url", "/" } }
        };
    });
}

// Redirect HTTP to HTTPS
app.UseHttpsRedirection();

// Create Uploads directory if it doesn't exist
var uploadsPath = Path.Combine(Directory.GetCurrentDirectory(), "Uploads");
if (!Directory.Exists(uploadsPath))
{
    Directory.CreateDirectory(uploadsPath);
}

// Serve static files from the "Uploads" directory
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(uploadsPath),
    RequestPath = "/uploads"
});

// Use refined CORS policy
app.UseCors("AllowFrontend");

app.UseAuthentication();
app.UseAuthorization();

// Map SignalR hub
app.MapHub<ActivityHub>("/ActivityHub").RequireCors("AllowFrontend");

// Map controller routes
app.MapControllers();

// Use global error handler
app.UseExceptionHandler("/error");

app.Run();

// REMOVED: Migration service registrations - no longer needed
// builder.Services.AddScoped<IMigrationService, MigrationService>();
// builder.Services.AddScoped<IUserMigrationService, UserMigrationService>();

