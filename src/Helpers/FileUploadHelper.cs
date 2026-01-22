using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Http;

namespace Lisa.Helpers
{
    public static class FileUploadHelper
    {
        public static async Task<IFormFile> ConvertToFormFileAsync(IBrowserFile browserFile)
        {
            var stream = browserFile.OpenReadStream(maxAllowedSize: 10 * 1024 * 1024); // 10MB
            var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream);
            memoryStream.Position = 0;

            return new FormFile(memoryStream, 0, memoryStream.Length, browserFile.Name, browserFile.Name)
            {
                Headers = new HeaderDictionary(),
                ContentType = browserFile.ContentType
            };
        }
    }
}