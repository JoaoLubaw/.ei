using Pontuei.Api.Data;
using Pontuei.Api.Interfaces.Services;
using Pontuei.Api.Models;

public static class LoyaltyProgramLogoSeeder
{
    private static readonly Dictionary<string, (string FileName, string ContentType)> LogoMap = new()
    {
        { "Esfera",           ("Esfera.webp",            "image/webp") },
        { "Dotz",             ("Dotz.webp",               "image/webp") },
        { "Livelo",           ("Livelo.webp",             "image/webp") },
        { "Inter Loop",       ("InterLoop.webp",          "image/webp") },
        { "XP Investimentos", ("XPInvestimentos.webp",    "image/webp") },
        { "Átomos",           ("Atomos.webp",             "image/webp") },
        { "Smiles",           ("Smiles.webp",             "image/webp") },
        { "Latam Pass",       ("LatamPass.webp",         "image/webp") },
        { "Azul Fidelidade",  ("Azul.webp",               "image/webp") },
        { "Itaú",             ("Itau.webp",               "image/webp") },
        { "Caixa",            ("Caixa.webp",              "image/webp") },
        { "Stix",             ("Stix.webp",               "image/webp") },
    };

    public static async Task SeedAsync(IServiceProvider services, IConfiguration config)
    {
        using IServiceScope scope = services.CreateScope();

        PontueiDbContext dbContext = scope.ServiceProvider.GetRequiredService<PontueiDbContext>();
        IStorageService storageService = scope.ServiceProvider.GetRequiredService<IStorageService>();
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