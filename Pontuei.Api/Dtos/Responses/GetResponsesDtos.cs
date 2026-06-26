using Pontuei.Api.Dtos.Objects;

namespace Pontuei.Api.Dtos.Responses;

public class GetUsersResponseDto
{
    public int Page { get; set; }
    public int Size { get; set; }
    public int TotalElements { get; set; }
    public int TotalPages { get; set; }
    public required List<UserDto> Users { get; set; }
}

public class GetLoyaltyProgramsResponseDto
{
    public int Page { get; set; }
    public int Size { get; set; }
    public int TotalElements { get; set; }
    public int TotalPages { get; set; }
    public required List<LoyaltyProgramDto> LoyaltyPrograms { get; set; }
}

public class GetUserLoyaltyProgramsResponseDto
{
    public int Page { get; set; }
    public int Size { get; set; }
    public int TotalElements { get; set; }
    public int TotalPages { get; set; }
    public required List<UserLoyaltyProgramDto> UserLoyaltyPrograms { get; set; }
}

public class GetConfigurationsResponseDto
{
    public int Page { get; set; }
    public int Size { get; set; }
    public int TotalElements { get; set; }
    public int TotalPages { get; set; }
    public required List<ConfigurationDto> Configurations { get; set; }
}

public class GetNotificationsResponseDto
{
    public int Page { get; set; }
    public int Size { get; set; }
    public int TotalElements { get; set; }
    public int TotalPages { get; set; }
    public int UnreadCount { get; set; }
    public required List<NotificationDto> Notifications { get; set; }
}

public class GetTransactionsResponseDto
{
    public int Page { get; set; }
    public int Size { get; set; }
    public int TotalElements { get; set; }
    public int TotalPages { get; set; }
    public required List<TransactionDto> Transactions { get; set; }
}

public class GetTransactionMediasResponseDto
{
    public int Page { get; set; }
    public int Size { get; set; }
    public int TotalElements { get; set; }
    public int TotalPages { get; set; }
    public required List<TransactionMediaDto> TransactionMedias { get; set; }
}