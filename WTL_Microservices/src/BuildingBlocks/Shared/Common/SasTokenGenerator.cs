using Azure.Storage;
using Azure.Storage.Sas;
using Microsoft.Extensions.Options;
using Shared.Common.Interfaces;
using Shared.Configurations;
using Shared.SeedWork;

namespace Shared.Common
{
    public class SasTokenGenerator: ISasTokenGenerator
    {
        private readonly AzureBlobSettings _azureBlobSettings;

        public SasTokenGenerator(IOptions<AzureBlobSettings> azureBlobSettings)
        {
            _azureBlobSettings = azureBlobSettings.Value;
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

        public string GenerateCoverImageUriWithSas(string coverImageUrl)
        {
            if(string.IsNullOrEmpty(coverImageUrl)) return null;
            var (folderName, fileName) = Util.ExtractNamesFromUrl(coverImageUrl);
            var sasToken = GenerateSasToken(fileName, folderName);
            var blobUriWithSas = new Uri(coverImageUrl + "?" + sasToken).ToString();
            return blobUriWithSas;
        }
    }
}
