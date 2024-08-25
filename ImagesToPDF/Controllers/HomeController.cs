using ImagesToPDF.Interface;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.IO.Compression;

namespace ImagesToPDF.Controllers
{
    public class HomeController : Controller
    {
        private readonly MyServiceInterface _myService;
        public HomeController(MyServiceInterface myService)
        {
            _myService = myService; 
        }

        [HttpPost("Download")]
        public async Task<IActionResult> DownloadFile([Required] IEnumerable<IFormFile> images, [Required] IFormFile pdf, [Required] IFormFile excel)
        {
            try
            {
                var result = await _myService.Imageuploads(images, pdf, excel);


                if (result.Success== true)
                {
                    Dictionary<string, byte[]> files = (Dictionary<string, byte[]>)result.Content;
                    using MemoryStream zipStream = new();
                    using (ZipArchive zipArchive = new(zipStream, ZipArchiveMode.Create, true))
                    {
                        foreach (var myFile in files)
                        {
                            byte[] fileBytes = myFile.Value;
                            string fileName = $"{myFile.Key}.pdf";
                            ZipArchiveEntry entry = zipArchive.CreateEntry(fileName);
                            using Stream entryStream = entry.Open();
                            entryStream.Write(fileBytes, 0, fileBytes.Length);
                        }
                    }

                    MemoryStream resultStream = new(zipStream.ToArray());
                    resultStream.Seek(0, SeekOrigin.Begin);
                    return File(resultStream, "application/zip", "files.zip");
                }
                return BadRequest(result);

            }
            catch (Exception ex)
            {

                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }
    }
}
