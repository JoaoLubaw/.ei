namespace Pontuei.Api.Dtos;

/// <summary>
/// Payload for adding a media file to an existing transaction.
/// </summary>
public class AddMediaRequestDto
{
    public IFormFile? File { get; set; }
}