using System;
using System.Linq;
using System.Reflection;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.StackExchangeRedis;
using Microsoft.IdentityModel.Tokens;
using StockApp.Entities;
using StockApp.Services;
using StockApp.Options;
using StockApp.App.Chat;

namespace StockApp;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Register DbContext with SQLite
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            var connectionString = configuration.GetConnectionString("DefaultConnection");
            options.UseSqlite(connectionString);
        });

        // Configure Identity
        services.AddIdentity<ApplicationUser, IdentityRole>(options =>
        {
            // Password settings
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 6;
            options.Password.RequiredUniqueChars = 1;

            // Lockout settings
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;

            // User settings
            options.User.RequireUniqueEmail = true;
            options.SignIn.RequireConfirmedEmail = false; // Development için false, production'da true olmalı
        })
        .AddEntityFrameworkStores<ApplicationDbContext>()
        .AddDefaultTokenProviders();

        // Configure JWT
        var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() 
            ?? throw new InvalidOperationException("JWT options not configured");

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        var key = Encoding.UTF8.GetBytes(jwtOptions.SecretKey);
        if (key.Length < 32)
        {
            throw new InvalidOperationException("JWT SecretKey must be at least 32 characters long");
        }

        services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            options.SaveToken = true;
            options.RequireHttpsMetadata = false; // Development için false, production'da true olmalı
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtOptions.Issuer,
                ValidAudience = jwtOptions.Audience,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ClockSkew = TimeSpan.Zero, // Token expiration'ı tam zamanında kontrol et
                
                // Role claim mapping - JWT token'daki role claim'lerini doğru şekilde map et
                RoleClaimType = ClaimTypes.Role,
                NameClaimType = ClaimTypes.Name
            };
            
            // JWT token'dan role'leri doğru şekilde çıkar
            options.Events = new Microsoft.AspNetCore.Authentication.JwtBearer.JwtBearerEvents
            {
                OnTokenValidated = context =>
                {
                    // Role claim'lerini kontrol et ve ekle
                    var roleClaims = context.Principal?.Claims
                        .Where(c => c.Type == ClaimTypes.Role)
                        .ToList();
                    
                    if (roleClaims != null && roleClaims.Any())
                    {
                        var identity = context.Principal?.Identity as System.Security.Claims.ClaimsIdentity;
                        if (identity != null)
                        {
                            foreach (var roleClaim in roleClaims)
                            {
                                if (!identity.HasClaim(ClaimTypes.Role, roleClaim.Value))
                                {
                                    identity.AddClaim(new System.Security.Claims.Claim(ClaimTypes.Role, roleClaim.Value));
                                }
                            }
                        }
                    }
                    
                    return Task.CompletedTask;
                }
            };
        });

        // Authorization policies
        services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
            options.AddPolicy("ManagerOrAdmin", policy => policy.RequireRole("Manager", "Admin"));
            options.AddPolicy("UserOrAbove", policy => policy.RequireRole("User", "Manager", "Admin"));
            
            // Claim-based policies
            options.AddPolicy("CanCreateUser", policy => 
                policy.RequireAssertion(context =>
                    context.User.IsInRole("Admin") || 
                    context.User.IsInRole("Manager") ||
                    context.User.HasClaim("Permission", "CanCreateUser")
                ));
            
            options.AddPolicy("CanManageUsers", policy => 
                policy.RequireAssertion(context =>
                    context.User.IsInRole("Admin") ||
                    context.User.HasClaim("Permission", "CanManageUsers")
                ));
            
            options.AddPolicy("CanManageRoles", policy => 
                policy.RequireAssertion(context =>
                    context.User.IsInRole("Admin") ||
                    context.User.HasClaim("Permission", "CanManageRoles")
                ));
            
            options.AddPolicy("CanViewRoles", policy => 
                policy.RequireAssertion(context =>
                    context.User.IsInRole("Admin") ||
                    context.User.IsInRole("Manager") ||
                    context.User.HasClaim("Permission", "CanViewRoles") ||
                    context.User.HasClaim("Permission", "CanManageRoles")
                ));
            
            // Category policies
            options.AddPolicy("CanViewCategories", policy => 
                policy.RequireAssertion(context =>
                    context.User.IsInRole("Admin") ||
                    context.User.HasClaim("Permission", "CanViewCategories") ||
                    context.User.HasClaim("Permission", "CanManageCategories")
                ));
            
            options.AddPolicy("CanManageCategories", policy => 
                policy.RequireAssertion(context =>
                    context.User.IsInRole("Admin") ||
                    context.User.HasClaim("Permission", "CanManageCategories")
                ));
            
            // Product policies
            options.AddPolicy("CanViewProducts", policy => 
                policy.RequireAssertion(context =>
                    context.User.IsInRole("Admin") ||
                    context.User.HasClaim("Permission", "CanViewProducts") ||
                    context.User.HasClaim("Permission", "CanManageProducts")
                ));
            
            options.AddPolicy("CanManageProducts", policy => 
                policy.RequireAssertion(context =>
                    context.User.IsInRole("Admin") ||
                    context.User.HasClaim("Permission", "CanManageProducts")
                ));
            
            // Product Attribute policies
            options.AddPolicy("CanViewProductAttributes", policy => 
                policy.RequireAssertion(context =>
                    context.User.IsInRole("Admin") ||
                    context.User.HasClaim("Permission", "CanViewProductAttributes") ||
                    context.User.HasClaim("Permission", "CanManageProductAttributes")
                ));
            
            options.AddPolicy("CanManageProductAttributes", policy => 
                policy.RequireAssertion(context =>
                    context.User.IsInRole("Admin") ||
                    context.User.HasClaim("Permission", "CanManageProductAttributes")
                ));
            
            // Stock Movement policies
            options.AddPolicy("CanViewStockMovements", policy => 
                policy.RequireAssertion(context =>
                    context.User.IsInRole("Admin") ||
                    context.User.HasClaim("Permission", "CanViewStockMovements") ||
                    context.User.HasClaim("Permission", "CanManageStockMovements")
                ));
            
            options.AddPolicy("CanManageStockMovements", policy => 
                policy.RequireAssertion(context =>
                    context.User.IsInRole("Admin") ||
                    context.User.HasClaim("Permission", "CanManageStockMovements")
                ));
            
            // Todo policies
            options.AddPolicy("CanViewTodos", policy => 
                policy.RequireAssertion(context =>
                    context.User.IsInRole("Admin") ||
                    context.User.HasClaim("Permission", "CanViewTodos") ||
                    context.User.HasClaim("Permission", "CanManageTodos")
                ));
            
            options.AddPolicy("CanManageTodos", policy => 
                policy.RequireAssertion(context =>
                    context.User.IsInRole("Admin") ||
                    context.User.HasClaim("Permission", "CanManageTodos")
                ));
            
            // Location policies
            options.AddPolicy("CanViewLocations", policy => 
                policy.RequireAssertion(context =>
                    context.User.IsInRole("Admin") ||
                    context.User.HasClaim("Permission", "CanViewLocations") ||
                    context.User.HasClaim("Permission", "CanManageLocations")
                ));
            
            options.AddPolicy("CanManageLocations", policy => 
                policy.RequireAssertion(context =>
                    context.User.IsInRole("Admin") ||
                    context.User.HasClaim("Permission", "CanManageLocations")
                ));
            
            // Dashboard policy
            options.AddPolicy("CanViewDashboard", policy => 
                policy.RequireAssertion(context =>
                    context.User.IsInRole("Admin") ||
                    context.User.HasClaim("Permission", "CanViewDashboard")
                ));
            
            // Reports policy
            options.AddPolicy("CanViewReports", policy => 
                policy.RequireAssertion(context =>
                    context.User.IsInRole("Admin") ||
                    context.User.HasClaim("Permission", "CanViewReports")
                ));
            
            // Chat policy
            options.AddPolicy("CanUseChat", policy => 
                policy.RequireAssertion(context =>
                    context.User.IsInRole("Admin") ||
                    context.User.HasClaim("Permission", "CanUseChat")
                ));
        });

        // Register MediatR
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
        });

        // Configure Redis Cache
        var redisConnectionString = configuration.GetConnectionString("Redis");
        if (!string.IsNullOrEmpty(redisConnectionString))
        {
            services.AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnectionString;
                options.InstanceName = "StockApp:"; // Key prefix
            });
        }
        else
        {
            // Redis yoksa in-memory cache kullan (development için)
            services.AddDistributedMemoryCache();
        }

        // Register application services
        services.AddScoped<IMarkdownService, MarkdownService>();
        services.AddScoped<IPdfService, PdfService>();
        services.AddScoped<IExcelService, ExcelService>();
        services.AddScoped<IImageService, ImageService>();
        services.AddScoped<IChatIntentDetector, ChatIntentDetector>();
        services.AddScoped<IGeminiIntentClassifier, GeminiIntentClassifier>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<RolePermissionService>();
        services.AddScoped<ICacheService, CacheService>();

        // Configure Elasticsearch (required for search functionality)
        var elasticsearchConnectionString = configuration.GetConnectionString("Elasticsearch");
        if (string.IsNullOrEmpty(elasticsearchConnectionString))
        {
            throw new InvalidOperationException("Elasticsearch connection string is required. Please configure 'ConnectionStrings:Elasticsearch' in appsettings.json");
        }
        
        services.Configure<ElasticsearchOptions>(opt =>
        {
            opt.ConnectionString = elasticsearchConnectionString;
        });
        services.AddScoped<IElasticsearchService, ElasticsearchService>();

        services.Configure<GeminiOptions>(configuration.GetSection(GeminiOptions.SectionName));
        services.AddScoped<IGeminiService, GeminiService>();

        return services;
    }
}