using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Amazon.S3.Util;
using Microsoft.AspNetCore.Http;
using Pontuei.Api.Interfaces.Services;

namespace Pontuei.Api.Services;

public class MinioStorageService : IStorageService
{
    private readonly IAmazonS3 _s3Client;
    private readonly string _bucketMedia;
    private readonly string _bucketPrograms;
    private readonly ILogger<MinioStorageService> _logger;

    public MinioStorageService(IConfiguration configuration, ILogger<MinioStorageService> logger)
    {
        _logger = logger;

        string accessKey = configuration["Storage:AccessKey"] ?? throw new ArgumentNullException("Storage:AccessKey is not configured.");
        string secretKey = configuration["Storage:SecretKey"] ?? throw new ArgumentNullException("Storage:SecretKey is not configured.");
        string serviceUrl = configuration["Storage:Endpoint"] ?? throw new ArgumentNullException("Storage:Endpoint is not configured.");

        AmazonS3Config config = new AmazonS3Config
        {
            ServiceURL = serviceUrl,
            ForcePathStyle = true
        };

        _s3Client = new AmazonS3Client(accessKey, secretKey, config);
        _bucketMedia = configuration["Storage:BucketMedia"] ?? throw new ArgumentNullException("Storage:BucketMedia is not configured.");
        _bucketPrograms = configuration["Storage:BucketPrograms"] ?? throw new ArgumentNullException("Storage:BucketPrograms is not configured.");
    }

    public async Task<string> UploadFileAsync(IFormFile file, Guid userId, Guid transactionId)
    {
        await EnsureBucketExistsAsync(_bucketMedia);

        string fileExtension = Path.GetExtension(file.FileName);
        string uniqueFileName = $"{userId}/{transactionId}/{Guid.NewGuid()}{fileExtension}";

        using MemoryStream newMemoryStream = new MemoryStream();
        await file.CopyToAsync(newMemoryStream);

        TransferUtilityUploadRequest uploadRequest = new TransferUtilityUploadRequest
        {
            InputStream = newMemoryStream,
            Key = uniqueFileName,
            BucketName = _bucketMedia,
            ContentType = file.ContentType
        };

        TransferUtility fileTransferUtility = new TransferUtility(_s3Client);

        _logger.LogInformation("Initializing upload to MinIO. File: {FileName}", uniqueFileName);

        await fileTransferUtility.UploadAsync(uploadRequest);

        _logger.LogInformation("Upload completed to MinIO. File: {FileName}", uniqueFileName);

        return $"/{_bucketMedia}/{uniqueFileName}";
    }

    public async Task DeleteFileAsync(string fileUrl)
    {
        await EnsureBucketExistsAsync(_bucketMedia);

        string fileKey = fileUrl.Replace($"/{_bucketMedia}/", "");

        DeleteObjectRequest deleteObjectRequest = new DeleteObjectRequest
        {
            BucketName = _bucketMedia,
            Key = fileKey
        };

        await _s3Client.DeleteObjectAsync(deleteObjectRequest);
        _logger.LogInformation("File deleted from MinIO. Key: {FileKey}", fileKey);
    }

    public async Task<string> UploadFileFromStreamAsync(Stream stream, string fileName, string contentType)
    {
        await EnsureBucketExistsAsync(_bucketPrograms, true);

        string key = $"loyalty-programs/{fileName}";

        TransferUtilityUploadRequest uploadRequest = new TransferUtilityUploadRequest
        {
            InputStream = stream,
            Key = key,
            BucketName = _bucketPrograms,
            ContentType = contentType,
            CannedACL = S3CannedACL.PublicRead
        };

        TransferUtility fileTransferUtility = new TransferUtility(_s3Client);
        await fileTransferUtility.UploadAsync(uploadRequest);

        return $"/{_bucketPrograms}/{key}";
    }

    private async Task EnsureBucketExistsAsync(string bucketName, bool publicRead = false)
    {
        bool exists = await AmazonS3Util.DoesS3BucketExistV2Async(_s3Client, bucketName);
        if (!exists)
        {
            await _s3Client.PutBucketAsync(new PutBucketRequest
            {
                BucketName = bucketName,
                UseClientRegion = true
            });
            _logger.LogInformation("Bucket created: {BucketName}", bucketName);
        }

        if (publicRead)
        {
            string policy = $$"""
        {
            "Version": "2012-10-17",
            "Statement": [{
                "Effect": "Allow",
                "Principal": {"AWS": ["*"]},
                "Action": ["s3:GetObject"],
                "Resource": ["arn:aws:s3:::{{bucketName}}/*"]
            }]
        }
        """;

            await _s3Client.PutBucketPolicyAsync(new PutBucketPolicyRequest
            {
                BucketName = bucketName,
                Policy = policy
            });
            _logger.LogInformation("Public read policy applied: {BucketName}", bucketName);
        }
    }
}