namespace Pontuei.Api.Interfaces.Services;

public interface IStorageService
{
    /// <summary>
    /// Uploads a file to the storage bucket and returns the public URL of the uploaded file.
    /// </summary>
    /// <param name="file"></param>
    /// <param name="userId"></param>
    /// <param name="transactionId"></param>
    /// <returns></returns>
    Task<string> UploadFileAsync(IFormFile file, Guid userId, Guid transactionId);

    /// <summary>
    /// Deletes a file from the storage bucket by its public URL.
    /// Returns <c>false</c> when the file was not found in the bucket.
    /// </summary>
    /// <param name="fileUrl"></param>
    /// <returns></returns>
    Task DeleteFileAsync(string fileUrl);

    /// <summary>
    /// Uploads a file to the storage bucket from a <see cref="Stream"/> and returns the public URL of the uploaded file.
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="fileName"></param>
    /// <param name="contentType"></param>
    /// <returns></returns>
    Task<string> UploadFileFromStreamAsync(Stream stream, string fileName, string contentType);

    /// <summary>
    /// Generates a pre-signed URL for accessing a file in the storage bucket.
    /// </summary>
    /// <param name="bucketName"></param>
    /// <param name="objectKey"></param>
    /// <param name="expirationInMinutes"></param>
    /// <returns></returns>
    string GeneratePreSignedUrl(string bucketName, string objectKey, int expirationInMinutes = 60);
}