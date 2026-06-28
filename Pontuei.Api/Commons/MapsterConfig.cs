using Mapster;
using Pontuei.Shared.Dtos.Objects;
using Pontuei.Api.Models;

namespace Pontuei.Api.Common;

public static class MapsterConfig
{
    public static void RegisterMappings()
    {
        TypeAdapterConfig<Configuration, ConfigurationDto>.NewConfig();
        TypeAdapterConfig<Notification, NotificationDto>.NewConfig();
        TypeAdapterConfig<Transaction, TransactionDto>.NewConfig();
        TypeAdapterConfig<TransactionMedia, TransactionMediaDto>.NewConfig();
        TypeAdapterConfig<User, UserDto>.NewConfig();
        TypeAdapterConfig<LoyaltyProgram, LoyaltyProgramDto>.NewConfig();
        TypeAdapterConfig<UserLoyaltyProgram, UserLoyaltyProgramDto>.NewConfig()
        .Map(dest => dest.LoyaltyProgram, src => src.LoyaltyProgram.Adapt<LoyaltyProgramDto>());

        TypeAdapterConfig.GlobalSettings.Default.MaxDepth(10);
        TypeAdapterConfig.GlobalSettings.Default.PreserveReference(true);
        TypeAdapterConfig.GlobalSettings.Scan(typeof(DbVersion).Assembly);
    }
}