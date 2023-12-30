namespace AzureBlob.API.Models
{
    public class BlobItemResponse
    {
        public string? FilePath { get; set; }
        public string? Name { get; set; }
        public Stream? Content { get; set; }
        public string? Message { get; set; }
        public string? ContentType { get; set; }
    }
}
