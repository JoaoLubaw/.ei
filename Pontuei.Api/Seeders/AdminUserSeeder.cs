using Pontuei.Api.Data;
using Pontuei.Api.Interfaces.Repositories;
using Pontuei.Api.Models;

public static class AdminUserSeeder
{
    public static async Task SeedAsync(IServiceProvider services, IConfiguration config)
    {
        using IServiceScope scope = services.CreateScope();
        PontueiDbContext dbContext = scope.ServiceProvider.GetRequiredService<PontueiDbContext>();
        IUserRepository userRepo = scope.ServiceProvider.GetRequiredService<IUserRepository>();
        IUnitOfWork unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();
        ILogger<Program> logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        string adminEmail = config["AdminSeed:Email"] ?? "admin@pontuei.com";
        string adminPassword = config["AdminSeed:Password"]
            ?? throw new InvalidOperationException("AdminSeed:Password não configurado.");

        User? existing = await userRepo.GetByEmailAsync(adminEmail);
        if (existing != null)
        {
            logger.LogInformation("Admin user already exists.");
            return;
        }

        logger.LogInformation("Seeding admin user with email: {Email}", adminEmail);

        string passwordHash = BCrypt.Net.BCrypt.HashPassword(adminPassword);

        User admin = new()
        {
            UserName = "Admin",
            UserEmail = adminEmail,
            UserPasswordHash = passwordHash,
            UserIsAdmin = true,
            UserEmailVerified = true,
            UserEmailVerifiedAt = DateTime.UtcNow,
            UserEmailNotificationsEnabled = false,
            UserPushNotificationsEnabled = false,
            UserAcceptedTerms = true,
            UserAcceptedTermsAt = DateTime.UtcNow,
            UserAcceptedTermsVersion = "1.0.0",
            CreationTime = DateTime.UtcNow,
            CreationUser = "seed"
        };

        dbContext.Users.Add(admin);
        await unitOfWork.CommitAsync();

        logger.LogInformation("Admin user seeded successfully.");
    }
}