using AzureBlob.API.Models;
using Microsoft.AspNetCore.Mvc;

namespace AzureBlob.API.Repositories.Interfaces
{
    public interface IAzureBlobRepository
    {
        Task<IActionResult> GetAttachment(string fileName, string folderName);
        Task<IActionResult> GetListAsync(string folderName);
        Task<IActionResult> UploadFile(IFormFile attachment, string folderName);
        Task<BlobItemResponse> DownloadAsync(string fileName, string folderName);
        Task<IActionResult> DeleteAsync(string fileName, string folderName);
        string GenerateSasToken(string fileName, string folderName);
    }
}
