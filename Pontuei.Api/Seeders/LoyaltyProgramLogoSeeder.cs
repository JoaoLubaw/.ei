using Pontuei.Api.Data;
using Pontuei.Api.Interfaces.Services;
using Pontuei.Api.Models;

public static class LoyaltyProgramLogoSeeder
{
    private static readonly Dictionary<string, (string FileName, string ContentType)> LogoMap = new()
    {
        { "Esfera",           ("Esfera.avif",            "image/avif") },
        { "Dotz",             ("Dotz.svg",               "image/svg+xml") },
        { "Livelo",           ("Livelo.svg",             "image/svg+xml") },
        { "Inter Loop",       ("InterLoop.svg",          "image/svg+xml") },
        { "XP Investimentos", ("XPInvestimentos.svg",    "image/svg+xml") },
        { "Átomos",           ("Atomos.svg",             "image/svg+xml") },
        { "Smiles",           ("Smiles.svg",             "image/svg+xml") },
        { "Latam Pass",       ("LatamPass.webp",         "image/webp") },
        { "Azul Fidelidade",  ("Azul.svg",               "image/svg+xml") },
        { "Itaú",             ("Itau.svg",               "image/svg+xml") },
        { "Caixa",            ("Caixa.svg",              "image/svg+xml") },
        { "Stix",             ("Stix.svg",               "image/svg+xml") },
    };

    public static async Task SeedAsync(IServiceProvider services, IConfiguration config)
    {
        using IServiceScope scope = services.CreateScope();

        PontueiDbContext dbContext = scope.ServiceProvider.GetRequiredService<PontueiDbContext>();
        IStorageService storageService = scope.ServiceProvider.GetRequiredService<IStorageService>(); // ← aqui
        ILogger<Program> logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();

        string assetsPath = Path.Combine(
            Directory.GetCurrentDirectory(), "assets", "LoyaltyPrograms");

        if (!Directory.Exists(assetsPath))
        {
            logger.LogWarning("Assets directory not found: {Path}. Ignoring logo seed.", assetsPath);
            return;
        }

        List<LoyaltyProgram> programs = dbContext.LoyaltyPrograms.ToList();

        foreach (LoyaltyProgram program in programs)
        {
            if (!string.IsNullOrEmpty(program.LoyaltyProgramLogoUrl)) continue;
            if (!LogoMap.TryGetValue(program.LoyaltyProgramName, out (string FileName, string ContentType) logoInfo)) continue;

            string filePath = Path.Combine(assetsPath, logoInfo.FileName);
            if (!File.Exists(filePath))
            {
                logger.LogWarning("File not found: {FilePath}", filePath);
                continue;
            }

            await using FileStream stream = File.OpenRead(filePath);
            string logoUrl = await storageService.UploadFileFromStreamAsync(
                stream, logoInfo.FileName, logoInfo.ContentType);

            program.LoyaltyProgramLogoUrl = logoUrl;
            dbContext.Entry(program).Property(p => p.LoyaltyProgramLogoUrl).IsModified = true;

            logger.LogInformation("Logo uploaded: {Program} → {Url}",
                program.LoyaltyProgramName, logoUrl);
        }

        await dbContext.SaveChangesAsync();
        logger.LogInformation("Logo seed completed.");
    }
}