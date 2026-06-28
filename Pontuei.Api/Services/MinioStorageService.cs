using Amazon.S3;
using Amazon.S3.Model;
using Amazon.S3.Transfer;
using Amazon.S3.Util;

using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;

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

        Stream uploadStream;
        string contentType = file.ContentType;

        try
        {
            using Image image = await Image.LoadAsync(file.OpenReadStream());

            MemoryStream outputStream = new MemoryStream();
            await image.SaveAsync(outputStream, new WebpEncoder());
            outputStream.Position = 0;

            uploadStream = outputStream;
            contentType = "image/webp";
            _logger.LogInformation("Image converted to WebP successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogInformation("File is not a processable image, keeping original format. Error: {Msg}", ex.Message);
            uploadStream = file.OpenReadStream();
        }

        TransferUtilityUploadRequest uploadRequest = new TransferUtilityUploadRequest
        {
            InputStream = uploadStream,
            Key = uniqueFileName,
            BucketName = _bucketMedia,
            ContentType = contentType
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

        Stream uploadStream = stream;
        string finalContentType = contentType;
        string finalFileName = fileName;

        try
        {
            using Image image = await Image.LoadAsync(stream);

            MemoryStream outputStream = new MemoryStream();
            await image.SaveAsync(outputStream, new WebpEncoder());
            outputStream.Position = 0;

            uploadStream = outputStream;
            finalContentType = "image/webp";
            finalFileName = Path.ChangeExtension(fileName, ".webp");
            _logger.LogInformation("Logo converted to WebP: {FileName}", finalFileName);
        }
        catch (Exception ex)
        {
            _logger.LogInformation("File is not a processable image or conversion failed. Keeping original: {Msg}", ex.Message);
        }

        string key = $"loyalty-programs/{finalFileName}";

        TransferUtilityUploadRequest uploadRequest = new TransferUtilityUploadRequest
        {
            InputStream = uploadStream,
            Key = key,
            BucketName = _bucketPrograms,
            ContentType = finalContentType,
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

    public string GeneratePreSignedUrl(string bucketName, string objectKey, int expirationInMinutes = 60)
    {
        if (objectKey.StartsWith("/"))
        {
            objectKey = objectKey.Substring(1);
        }

        if (objectKey.StartsWith(bucketName + "/"))
        {
            objectKey = objectKey.Substring(bucketName.Length + 1);
        }

        GetPreSignedUrlRequest request = new GetPreSignedUrlRequest
        {
            BucketName = bucketName,
            Key = objectKey,
            Expires = DateTime.UtcNow.AddMinutes(expirationInMinutes)
        };

        string url = _s3Client.GetPreSignedURL(request);

        // --- DOCKER TREATMENT ---
        // When the API (inside Docker) generates the URL, MinIO uses the internal network name
        // docker, ex: http://pontuei-minio:9000/...
        // But the MAUI application (running in the emulator or on the phone) doesn't understand "pontuei-minio", 
        // it needs to access via localhost (or 10.0.2.2 on Android).
        // 
        // So, we do a replace only for local development:
        string localMinioUrl = "http://pontuei-minio:9000";
        string exposedMinioUrl = "http://localhost:9000";

        if (url.StartsWith(localMinioUrl))
        {
            url = url.Replace(localMinioUrl, exposedMinioUrl);
        }

        return url;
    }
}