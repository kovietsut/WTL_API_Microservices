using AzureBlob.API.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AzureBlob.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class AzureBlobController : ControllerBase
    {
        private readonly IAzureBlobRepository _azureBlobRepository;

        public AzureBlobController(IAzureBlobRepository azureBlobRepository)
        {
            _azureBlobRepository = azureBlobRepository;
        }

        [HttpGet("{folderName}/{fileName}")]
        public async Task<IActionResult> GetAttachment(string fileName, string folderName)
        {
            return await _azureBlobRepository.GetAttachment(fileName, folderName);
        }

        [HttpGet]
        public async Task<IActionResult> GetList(string folderName)
        {
            return await _azureBlobRepository.GetListAsync(folderName);
        }

        [HttpPost]
        public async Task<IActionResult> Upload(IFormFile attachment, string folderName)
        {
            return await _azureBlobRepository.UploadFile(attachment, folderName);
        }

        [HttpPost("upload-list")]
        public async Task<IActionResult> UploadList(IFormFileCollection attachment, string folderName)
        {
            return await _azureBlobRepository.UploadListFile(attachment, folderName);
        }

        [HttpDelete]
        public async Task<IActionResult> Delete(string fileName, string folderName)
        {
            return await _azureBlobRepository.DeleteAsync(fileName, folderName);
        }
    }
}
