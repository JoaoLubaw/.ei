using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Microsoft.AspNetCore.Http;
using Pontuei.Api.Interfaces.Services;

namespace Pontuei.Api.Services;

public class MinioStorageService : IStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketName = "pontuei-media";
    private readonly ILogger<MinioStorageService> _logger;

    public MinioStorageService(IConfiguration configuration, ILogger<MinioStorageService> logger)
    {
        _logger = logger;

        string accessKey = configuration["MinIO:AccessKey"] ?? throw new ArgumentNullException("MinIO:AccessKey is not configured.");
        string secretKey = configuration["MinIO:SecretKey"] ?? throw new ArgumentNullException("MinIO:SecretKey is not configured.");
        string serviceUrl = configuration["MinIO:Endpoint"] ?? throw new ArgumentNullException("MinIO:Endpoint is not configured.");

        AmazonS3Config config = new AmazonS3Config
        {
            ServiceURL = serviceUrl,
            ForcePathStyle = true
        };

        _s3Client = new AmazonS3Client(accessKey, secretKey, config);
    }

    public async Task<string> UploadFileAsync(IFormFile file, Guid userId, Guid transactionId)
    {
        string fileExtension = Path.GetExtension(file.FileName);
        string uniqueFileName = $"{userId}/{transactionId}/{Guid.NewGuid()}{fileExtension}";

        using MemoryStream newMemoryStream = new MemoryStream();
        await file.CopyToAsync(newMemoryStream);

        TransferUtilityUploadRequest uploadRequest = new TransferUtilityUploadRequest
        {
            InputStream = newMemoryStream,
            Key = uniqueFileName,
            BucketName = _bucketName,
            ContentType = file.ContentType
        };

        TransferUtility fileTransferUtility = new TransferUtility(_s3Client);

        _logger.LogInformation("Initializing upload to MinIO. File: {FileName}", uniqueFileName);

        await fileTransferUtility.UploadAsync(uploadRequest);

        _logger.LogInformation("Upload completed to MinIO. File: {FileName}", uniqueFileName);

        return $"/{_bucketName}/{uniqueFileName}";
    }

    public async Task DeleteFileAsync(string fileUrl)
    {
        string fileKey = fileUrl.Replace($"/{_bucketName}/", "");

        DeleteObjectRequest deleteObjectRequest = new DeleteObjectRequest
        {
            BucketName = _bucketName,
            Key = fileKey
        };

        await _s3Client.DeleteObjectAsync(deleteObjectRequest);
        _logger.LogInformation("File deleted from MinIO. Key: {FileKey}", fileKey);
    }
}