using ImageResize.Shared;
using System.ComponentModel.DataAnnotations;

namespace ImagesToPDF.Interface
{
    public interface MyServiceInterface
    {
        Task<Result<object>> Imageuploads([Required] IEnumerable<IFormFile> images, [Required] IFormFile pdf, [Required] IFormFile excel);
    }
}
