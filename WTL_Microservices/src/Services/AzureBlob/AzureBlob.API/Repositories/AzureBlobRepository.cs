using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using AzureBlob.API.Models;
using AzureBlob.API.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Shared.Configurations;
using Shared.DTOs;
using Shared.SeedWork;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using System.Text;

namespace AzureBlob.API.Repositories
{
    public class AzureBlobRepository: IAzureBlobRepository
    {
        private readonly AzureBlobSettings _azureBlobSettings;
        private readonly BlobServiceClient _blobServiceClient;
        private readonly ErrorCode _errorCodes;

        public AzureBlobRepository(IOptions<AzureBlobSettings> azureBlobSettings, IOptions<ErrorCode> errorCodes)
        {
            _azureBlobSettings = azureBlobSettings.Value;
            _errorCodes = errorCodes.Value;
            var sharedKeyCredential = new StorageSharedKeyCredential(_azureBlobSettings.AccountName, _azureBlobSettings.AccountKey);
            var blobUri = $"https://{_azureBlobSettings.AccountName}.blob.core.windows.net";
            _blobServiceClient = new BlobServiceClient(new Uri(blobUri), sharedKeyCredential);
        }

        public string GenerateSasToken(string fileName, string folderName)
        {
            var startTime = DateTimeOffset.UtcNow;
            var expiryTime = DateTimeOffset.UtcNow.AddMinutes(30);
            // Generate the SAS token
            var sasBuilder = new BlobSasBuilder()
            {
                BlobContainerName = folderName,
                BlobName = fileName,
                Resource = "b", // "b" indicates a blob-level SAS
                StartsOn = startTime,
                ExpiresOn = expiryTime,
            };
            sasBuilder.SetPermissions(BlobSasPermissions.Read);
            var sasToken = sasBuilder.ToSasQueryParameters(new StorageSharedKeyCredential(_azureBlobSettings.AccountName,
            _azureBlobSettings.AccountKey)).ToString();
            return sasToken;
        }

        public async Task<IActionResult> GetAttachment(string fileName, string folderName)
        {
            try
            {
                if (folderName == null)
                {
                    return JsonUtil.Error(StatusCodes.Status400BadRequest, _errorCodes.Status400.Notfound, "Folder name is required");
                }
                if (fileName == null)
                {
                    return JsonUtil.Error(StatusCodes.Status400BadRequest, _errorCodes.Status400.Notfound, "File name is required");
                }
                var file = _blobServiceClient.GetBlobContainerClient(folderName).GetBlobClient(fileName);
                if (await file.ExistsAsync())
                {
                    var sasToken = GenerateSasToken(fileName, folderName);
                    var blobUriWithSas = new Uri(file.Uri + "?" + sasToken).ToString();
                    var data = await file.OpenReadAsync();
                    var content = await file.DownloadContentAsync();
                    var contentType = content.Value.Details.ContentType;
                    return JsonUtil.Success(new
                    {
                        FilePath = blobUriWithSas,
                        file.Name,
                        ContentType = contentType
                    });
                }
                return JsonUtil.Error(StatusCodes.Status400BadRequest, _errorCodes.Status400.Notfound, "File does not exist");
            }
            catch (Exception e)
            {
                return JsonUtil.Error(StatusCodes.Status500InternalServerError, _errorCodes.Status500.ServerError, e.Message);
            }
        }

        public async Task<IActionResult> GetListAsync(string folderName)
        {
            try
            {
                if (folderName == null)
                {
                    return JsonUtil.Error(StatusCodes.Status400BadRequest, _errorCodes.Status400.Notfound, "Folder name is required");
                }
                var containerClient = _blobServiceClient.GetBlobContainerClient(folderName);
                if (await containerClient.ExistsAsync() != true)
                {
                    return JsonUtil.Error(StatusCodes.Status400BadRequest, _errorCodes.Status400.Notfound, "Folder does not exist");
                }
                var blobs = new List<BlobItemResponse>();
                var uri = _blobServiceClient.Uri.ToString();
                await foreach (var blobItem in containerClient.GetBlobsAsync())
                {
                    var sasToken = GenerateSasToken(blobItem.Name, folderName);
                    var blobUriWithSas = new Uri($"{uri}{folderName}/{blobItem.Name}" + "?" + sasToken).ToString();
                    blobs.Add(new BlobItemResponse
                    {
                        FilePath = blobUriWithSas,
                        Name = blobItem.Name,
                        ContentType = blobItem.Properties.ContentType
                    });
                }
                return JsonUtil.Success(blobs);
            }
            catch (Exception e)
            {
                return JsonUtil.Error(StatusCodes.Status500InternalServerError, _errorCodes.Status500.ServerError, e.Message);
            }
        }

        private async Task CompressAsync(IFormFile imageFile, string outputPath, int quality)
        {
            using var imageStream = imageFile.OpenReadStream();
            using var image = Image.Load(imageStream);
            var encoder = new JpegEncoder
            {
                Quality = quality,
            };
            await Task.Run(() => image.Save(outputPath, encoder));
        }

        public async Task<IActionResult> UploadFile(IFormFile attachment, string folderName)
        {
            try
            {
                var quality = 75;
                if (folderName == null)
                {
                    return JsonUtil.Error(StatusCodes.Status400BadRequest, _errorCodes.Status400.BadRequest, "Folder name cannot be null");
                }
                if (attachment == null)
                {
                    return JsonUtil.Error(StatusCodes.Status400BadRequest, _errorCodes.Status400.BadRequest, "Attachment cannot be null");
                }
                // Create temporary file and Compress file
                var tempFilePath = Path.GetTempFileName();
                await CompressAsync(attachment, tempFilePath, quality);
                // Upload file to blob Azure
                var containerClient = _blobServiceClient.GetBlobContainerClient(folderName);
                await containerClient.CreateIfNotExistsAsync();
                string uniqueAttachmentName = Guid.NewGuid().ToString() + "9999a" + attachment.FileName;
                var blobClient = containerClient.GetBlobClient(uniqueAttachmentName);
                var compressedFileStream = File.OpenRead(tempFilePath);
                await blobClient.UploadAsync(compressedFileStream, new BlobHttpHeaders { ContentType = attachment.ContentType });
                // Delete the temporary file
                // File.Delete(tempFilePath);
                return JsonUtil.Success(new
                {
                    FilePath = blobClient.Uri.ToString(),
                    attachment.FileName,
                    attachment.ContentType
                });
            }
            catch (Exception e)
            {
                return JsonUtil.Error(StatusCodes.Status500InternalServerError, _errorCodes.Status500.ServerError, e.Message);
            }
        }

        public async Task<IActionResult> UploadListFile(IFormFileCollection attachments, string folderName)
        {
            try
            {
                var quality = 75;
                if (folderName == null)
                {
                    return JsonUtil.Error(StatusCodes.Status400BadRequest, _errorCodes.Status400.BadRequest, "Folder name cannot be null");
                }
                if (attachments == null || attachments.Count() <= 0)
                {
                    return JsonUtil.Error(StatusCodes.Status400BadRequest, _errorCodes.Status400.BadRequest, "Please select at least 1 attachment");
                }
                // Create temporary file and Compress file
                foreach ( var attachment in attachments)
                {
                    var tempFilePath = Path.GetTempFileName();
                    await CompressAsync(attachment, tempFilePath, quality);
                    // Upload file to blob Azure
                    var containerClient = _blobServiceClient.GetBlobContainerClient(folderName);
                    await containerClient.CreateIfNotExistsAsync();
                    string uniqueAttachmentName = Guid.NewGuid().ToString() + "9999a" + attachment.FileName;
                    var blobClient = containerClient.GetBlobClient(uniqueAttachmentName);
                    var compressedFileStream = File.OpenRead(tempFilePath);
                    await blobClient.UploadAsync(compressedFileStream, new BlobHttpHeaders { ContentType = attachment.ContentType });
                    // Delete the temporary file
                    // File.Delete(tempFilePath);
                }
                return JsonUtil.Success($"Upload {attachments.Count()} Success");
            }
            catch (Exception e)
            {
                return JsonUtil.Error(StatusCodes.Status500InternalServerError, _errorCodes.Status500.ServerError, e.Message);
            }
        }

        public async Task<IActionResult> DeleteAsync(string fileName, string folderName)
        {
            try
            {
                var file = _blobServiceClient.GetBlobContainerClient(folderName).GetBlobClient(fileName);
                await file.DeleteAsync();
                return JsonUtil.Success($"File {fileName} has been deleted");
            }
            catch (Exception e)
            {
                return JsonUtil.Error(StatusCodes.Status500InternalServerError, _errorCodes.Status500.ServerError, e.Message);
            }
        }
    }
}
