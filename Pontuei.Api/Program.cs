using System.Text;
using System.Threading.RateLimiting;
using FirebaseAdmin;
using Google.Apis.Auth.OAuth2;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Pontuei.Api.Data;
using Pontuei.Api.Interfaces.Jobs;
using Pontuei.Api.Interfaces.Repositories;
using Pontuei.Api.Interfaces.Services;
using Pontuei.Api.Jobs;
using Pontuei.Api.Models.Settings;
using Pontuei.Api.Repositories;
using Pontuei.Api.Services;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, configuration) =>
    configuration.ReadFrom.Configuration(context.Configuration));

try
{
    Log.Information("Iniciando a API do Pontuei...");

    // =========================================================
    // FIREBASE ADMIN SDK
    // Used for sending push notifications via FCM (Firebase Cloud Messaging)
    // and to login users via Google OAuth.
    // =========================================================
    string firebaseCredentialsJson = Environment.GetEnvironmentVariable("FIREBASE_CREDENTIALS_JSON")
        ?? builder.Configuration["Firebase:CredentialsJson"]
        ?? throw new InvalidOperationException(
            "Firebase credentials not found. Set FIREBASE_CREDENTIALS_JSON environment variable.");

    using MemoryStream credentialStream = new MemoryStream(Encoding.UTF8.GetBytes(firebaseCredentialsJson));

    ServiceAccountCredential serviceAccountCredential =
        ServiceAccountCredential.FromServiceAccountData(credentialStream);

    FirebaseApp.Create(new AppOptions
    {
        Credential = GoogleCredential
            .FromServiceAccountCredential(serviceAccountCredential)
            .CreateScoped("https://www.googleapis.com/auth/firebase.messaging")
    });


    Log.Information("Firebase Admin SDK inicializado com sucesso.");

    // =========================================================
    // DATABASE — Entity Framework Core + PostgreSQL
    // =========================================================
    builder.Services.AddDbContext<PontueiDbContext>(options =>
        options.UseNpgsql(builder.Configuration.GetConnectionString("PontueiDb")));

    // =========================================================
    // CACHE — Redis (StackExchange)
    // =========================================================
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = builder.Configuration.GetConnectionString("Redis");
    });

    // =========================================================
    // AUTHENTICATION — JWT Bearer
    // =========================================================
    IConfigurationSection jwtSettings = builder.Configuration.GetSection("JWT");
    byte[] secretKey = Encoding.UTF8.GetBytes(jwtSettings["Secret"]!);

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(options =>
        {
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = jwtSettings["Issuer"],
                ValidAudience = jwtSettings["Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(secretKey)
            };
        });

    builder.Services.AddAuthorization();

    // =========================================================
    // SETTINGS — strongly-typed configuration
    // =========================================================
    builder.Services.Configure<EmailSettings>(
        builder.Configuration.GetSection("EmailSettings"));

    // =========================================================
    // RATE LIMITING 
    // =========================================================
    builder.Services.AddRateLimiter(options =>
    {
        // Politic 1: Global / Generic
        options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        {
            string ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";
            return RateLimitPartition.GetFixedWindowLimiter(ipAddress, partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1)
            });
        });

        // Politic 2: To sensitive routes
        options.AddPolicy("StrictAuthLimit", httpContext =>
        {
            string ipAddress = httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown";

            return RateLimitPartition.GetFixedWindowLimiter(
                partitionKey: ipAddress,
                factory: partition => new FixedWindowRateLimiterOptions
                {
                    AutoReplenishment = true,
                    PermitLimit = 3,
                    Window = TimeSpan.FromMinutes(1)
                });
        });

        options.OnRejected = async (context, token) =>
        {
            context.HttpContext.Response.StatusCode = 429;
            context.HttpContext.Response.ContentType = "application/json";

            var errorResponse = new
            {
                resultCode = "TOO_MANY_REQUESTS",
                httpCode = 429,
                message = "Muitas tentativas em pouco tempo. Por favor, aguarde alguns instantes."
            };

            await context.HttpContext.Response.WriteAsJsonAsync(errorResponse, token);
        };
    });

    // =========================================================
    // REPOSITORIES
    // =========================================================
    builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
    builder.Services.AddScoped<IUserRepository, UserRepository>();
    builder.Services.AddScoped<IUserSessionRepository, UserSessionRepository>();
    builder.Services.AddScoped<IVerificationCodeRepository, VerificationCodeRepository>();
    builder.Services.AddScoped<ILoyaltyProgramRepository, LoyaltyProgramRepository>();
    builder.Services.AddScoped<IUserLoyaltyProgramRepository, UserLoyaltyProgramRepository>();
    builder.Services.AddScoped<ITransactionRepository, TransactionRepository>();
    builder.Services.AddScoped<ITransactionMediaRepository, TransactionMediaRepository>();
    builder.Services.AddScoped<INotificationRepository, NotificationRepository>();
    builder.Services.AddScoped<IConfigurationRepository, ConfigurationRepository>();
    builder.Services.AddScoped<IMetadataRepository, MetadataRepository>();

    // =========================================================
    // SERVICES
    // =========================================================
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<IUserService, UserService>();
    builder.Services.AddScoped<ITokenService, TokenService>();
    builder.Services.AddScoped<IEmailService, EmailService>();
    builder.Services.AddScoped<ILoyaltyProgramService, LoyaltyProgramService>();
    builder.Services.AddScoped<IUserLoyaltyProgramService, UserLoyaltyProgramService>();
    builder.Services.AddScoped<ITransactionService, TransactionService>();
    builder.Services.AddScoped<INotificationService, NotificationService>();
    builder.Services.AddScoped<IStorageService, MinioStorageService>();
    builder.Services.AddScoped<IConfigurationService, ConfigurationService>();
    builder.Services.AddScoped<IMetadataService, MetadataService>();

    // Push notifications — Singleton pois o FirebaseMessaging.DefaultInstance já é singleton
    builder.Services.AddSingleton<IPushNotificationService, FcmPushNotificationService>();

    // =========================================================
    // BACKGROUND JOB — Overdue Transaction Checker
    // =========================================================
    builder.Services.AddScoped<IOverdueTransactionJob, OverdueTransactionJob>();
    builder.Services.AddHostedService<OverdueTransactionHostedService>();

    // =========================================================
    // CONTROLLERS + SWAGGER
    // =========================================================
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(options =>
    {
        options.SwaggerDoc("v1", new OpenApiInfo
        {
            Title = ".ei.API",
            Version = "v1",
            Description = "Nunca foi tão fácil colocar os pontos nos i's."
        });

        options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "Bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Informe o token JWT no formato: Bearer {token}"
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

        string xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
        string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
        if (File.Exists(xmlPath))
            options.IncludeXmlComments(xmlPath);
    });

    // =========================================================
    // CORS 
    // =========================================================
    builder.Services.AddCors(options =>
    {
        options.AddPolicy("AllowAll", policy =>
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader());
    });

    // =========================================================
    // BUILD
    // =========================================================
    WebApplication app = builder.Build();

    // =========================================================
    // MIDDLEWARE PIPELINE
    // =========================================================
    app.UseSerilogRequestLogging();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", ".ei API v1"));
    }

    app.UseCors("AllowAll");

    app.UseAuthentication();
    app.UseAuthorization();

    app.UseRateLimiter();

    app.MapControllers();

    Log.Information(".EI API Ready. Environment: {Environment}", app.Environment.EnvironmentName);

    await AdminUserSeeder.SeedAsync(app.Services, app.Configuration);
    await LoyaltyProgramLogoSeeder.SeedAsync(app.Services, app.Configuration);

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Failed to start the application.");
}
finally
{
    Log.CloseAndFlush();
}