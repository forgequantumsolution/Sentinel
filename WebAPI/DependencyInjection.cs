using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using Infrastructure.Persistence;
using Infrastructure.Persistence.Repositories;
using Infrastructure.Security;
using Infrastructure.Services;
using Infrastructure.FormQuery;
using Application.Services;
using Application.Interfaces;
using Application.Interfaces.Persistence;
using Application.Interfaces.Services;
using Application.FormQuery;
using Microsoft.Extensions.Options;

namespace WebAPI
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            // Entity Framework (PostgreSQL)
            services.AddDbContext<AppDbContext>((sp, options) =>
            {
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"));

                var env = sp.GetRequiredService<IHostEnvironment>();
                if (env.IsDevelopment())
                {
                    options.EnableSensitiveDataLogging();
                    options.EnableDetailedErrors();
                    options.LogTo(Console.WriteLine, LogLevel.Information);
                }
            });

            // Repositories
            services.AddScoped<IOrganizationRepository, OrganizationRepository>();
            services.AddScoped<IUserRepository, UserRepository>();
            services.AddScoped<IRoleRepository, RoleRepository>();
            services.AddScoped<IDepartmentRepository, DepartmentRepository>();
            services.AddScoped<IJobTitleRepository, JobTitleRepository>();
            services.AddScoped<IUserGroupRepository, UserGroupRepository>();
            services.AddScoped<IDynamicFormRepository, DynamicFormRepository>();
            services.AddScoped<IDynamicFormSubmissionRepository, DynamicFormSubmissionRepository>();
            services.AddScoped<IDynamicFormDraftRepository, DynamicFormDraftRepository>();
            services.AddScoped<IGraphConfigRepository, GraphConfigRepository>();
            services.AddScoped<IGraphDataDefinitionRepository, GraphDataDefinitionRepository>();
            services.AddScoped<IDynamicGroupingRuleRepository, DynamicGroupingRuleRepository>();
            services.AddScoped<IDynamicGroupObjectPermissionRepository, DynamicGroupObjectPermissionRepository>();
            services.AddScoped<IBulkUploadJobRepository, BulkUploadJobRepository>();
            services.AddScoped<IActionObjectRepository, ActionObjectRepository>();
            services.AddScoped<IUploadedFileRepository, UploadedFileRepository>();
            services.AddScoped<IUserGroupMembershipRepository, UserGroupMembershipRepository>();

            // Security
            services.AddScoped<IPasswordHasher, PasswordHasher>();
            services.AddScoped<ITokenService, TokenService>();

            // Multi-tenancy
            services.AddScoped<ITenantContext, Infrastructure.Services.TenantContext>();

            // Background services
            services.AddHostedService<BulkUploadBackgroundService>();
            services.AddHostedService<WebAPI.Services.EndpointSyncService>();

            // Caching
            services.AddMemoryCache();

            // Form Query Engine
            services.AddScoped<IFormQueryEngine, FormQueryEngine>();

            // File Storage — transient so provider can vary per tenant in the future
            services.Configure<Application.Models.FileStorageSettings>(
                configuration.GetSection(Application.Models.FileStorageSettings.SectionName));

            services.AddTransient<IFileStorageService>(sp =>
            {
                var settings = sp.GetRequiredService<IOptions<Application.Models.FileStorageSettings>>();
                var provider = settings.Value.Provider;

                return provider?.ToLowerInvariant() switch
                {
                    "azureblob" => new Infrastructure.Services.AzureBlobStorageService(settings),
                    "cloudflarer2" => new Infrastructure.Services.CloudflareR2StorageService(settings),
                    "awss3" => new Infrastructure.Services.AwsS3StorageService(settings),
                    "googlecloudstorage" => new Infrastructure.Services.GoogleCloudStorageService(settings),
                    _ => new Infrastructure.Services.LocalFileStorageService(settings)
                };
            });

            // Encryption
            services.AddSingleton<IEncryptionService, Infrastructure.Services.AesEncryptionService>();

            // CSV Data Source
            services.AddScoped<ICsvDataSourceService, Infrastructure.Services.CsvDataSourceService>();

            // Excel Parser
            services.AddScoped<IExcelParserService, Infrastructure.Services.ExcelParserService>();

            return services;
        }

        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IUserService, UserService>();
            services.AddScoped<IAuthService, AuthService>();
            services.AddScoped<IPermissionService, PermissionService>();
            services.AddScoped<IRbacService, RbacService>(); 
            services.AddScoped<IGraphService, GraphService>();
            services.AddScoped<IDynamicGroupingRuleService, DynamicGroupingRuleService>();
            services.AddScoped<IDynamicGroupObjectPermissionService, DynamicGroupObjectPermissionService>();
            services.AddScoped<IBulkUploadService, BulkUploadService>();
            services.AddScoped<IAppFolderService, AppFolderService>();
            services.AddScoped<IGroupMembershipService, GroupMembershipService>();
            services.AddSingleton<IRuleFieldService, RuleFieldService>();
            services.AddScoped<IAutoGroupProvisioningService, AutoGroupProvisioningService>();

            return services;
        }

        public static IServiceCollection AddAuthenticationConfig(this IServiceCollection services, IConfiguration configuration)
        {
            var jwtSecretKey = configuration["Jwt:SecretKey"] ?? throw new InvalidOperationException("JWT Secret Key is not configured");
            var jwtIssuer = configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("JWT Issuer is not configured");
            var jwtAudience = configuration["Jwt:Audience"] ?? throw new InvalidOperationException("JWT Audience is not configured");

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.SaveToken = true;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtIssuer,
                        ValidAudience = jwtAudience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecretKey)),
                        ClockSkew = TimeSpan.Zero
                    };
                });

            services.AddAuthorization();

            return services;
        }

        public static IServiceCollection AddCorsConfig(this IServiceCollection services)
        {
            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", policy =>
                {
                    policy.AllowAnyOrigin()
                          .AllowAnyMethod()
                          .AllowAnyHeader();
                });
            });

            return services;
        }

        public static IServiceCollection AddSwaggerConfig(this IServiceCollection services)
        {
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "Analytics BE API",
                    Version = "v1"
                });

                options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey,
                    Scheme = "Bearer"
                });

                options.AddSecurityRequirement(new OpenApiSecurityRequirement
                {
                    {
                        new OpenApiSecurityScheme
                        {
                            Reference = new OpenApiReference
                            {
                                Type = ReferenceType.SecurityScheme,
                                Id = "Bearer"
                            }
                        },
                        Array.Empty<string>()
                    }
                });

                // Custom schema ID to avoid conflicts between duplicate type names
                options.CustomSchemaIds(type => type.FullName?.Replace("+", "."));
            });

            return services;
        }
    }
}
