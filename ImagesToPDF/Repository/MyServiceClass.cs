using ImageResize.Shared;
using ImagesToPDF.Interface;
using OfficeOpenXml;
using Syncfusion.Pdf;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Pdf.Parsing;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.Drawing.Drawing2D;

namespace ImagesToPDF.Repository
{
    public class MyServiceClass: MyServiceInterface
    {
        
        public async Task<Result<object>> Imageuploads([Required] IEnumerable<IFormFile> images, [Required] IFormFile pdf, [Required] IFormFile excel)
        {
            try
            {
                string pdfExtension = Path.GetExtension(pdf.FileName).ToLower();
                if (pdfExtension != ".pdf")
                {
                    return new ErrorResult($"{pdf?.FileName} is an invalid PDF.");
                }
                string excelfExtension = Path.GetExtension(excel.FileName).ToLower();
                if (!(excelfExtension == ".xlsx" || excelfExtension == ".xls"))
                {
                    return new ErrorResult($"{excel?.FileName} is an invalid excel file.");
                }
                if (pdf == null || pdf.Length == 0)
                    return new ErrorResult($"Upload a valid PDF");
                if (excel == null || excel.Length == 0)
                    return new ErrorResult($"Upload a valid excel file");
                //These are default values that works for this specfic project. thats why it was hard coded.
                int wid = 620;
                int hght = 620;
                int XX = 400;
                int YY = 600;
                Dictionary<string, byte[]> resultList = new();
                Syncfusion.Licensing.SyncfusionLicenseProvider.RegisterLicense("");
                var croppedImage = CropAndResizeMultiple(images, wid, hght, XX, YY);
                if (croppedImage.Success == false)
                {
                    return new ErrorResult($"{croppedImage.Message}");
                }
                Dictionary<string, byte[]> returnedImages = (Dictionary<string, byte[]>)croppedImage.Content;
                Dictionary<string, byte[]> updatedCroppedImage = new();
                foreach (var kvp in returnedImages)
                {
                    string name = kvp.Key.Substring(12);
                    string finalName = name.Substring(0, name.Length - 8);
                    string updatedKey = finalName.Replace("_", " ");
                    if (updatedCroppedImage.ContainsKey(updatedKey))
                    {
                        updatedCroppedImage.Remove(updatedKey);
                    }
                    updatedCroppedImage.Add(updatedKey, kvp.Value);
                }

                Dictionary<string, string> readExcel = ReadExcelFile(excel);

                foreach (var excelEntry in readExcel)
                {

                    string excelKey = excelEntry.Key;
                    string excelValue = excelEntry.Value;

                    // Check if the key exists in updatedCroppedImage
                    if (updatedCroppedImage.ContainsKey(excelKey))
                    {
                        byte[] imageValue = updatedCroppedImage[excelKey];

                        using PdfLoadedDocument doc = new(pdf.OpenReadStream());
                        PdfLoadedPage page = doc.Pages[0] as PdfLoadedPage;
                        PdfGraphics graphics = page.Graphics;

                        PdfFont font = new PdfStandardFont(PdfFontFamily.Helvetica, 70, PdfFontStyle.Bold);
                        PdfFont font2 = new PdfStandardFont(PdfFontFamily.Helvetica, 32, PdfFontStyle.Bold);
                        PdfStringFormat format = new()
                        {
                            //Set the text alignment.
                            Alignment = PdfTextAlignment.Center
                        };
                        //Create a new PDF string format instance.
                        graphics.DrawString(excelValue, font, PdfBrushes.Black, new Syncfusion.Drawing.RectangleF(0, 165, page.Size.Width, page.Size.Height), format);
                        graphics.DrawString(excelKey, font2, PdfBrushes.Black, new Syncfusion.Drawing.RectangleF(0, 500, page.Size.Width, page.Size.Height), format);

                        // Iterate over cropped images
                        using (MemoryStream memoryStream = new(imageValue))
                        {
                            memoryStream.Position = 0; // Reset the position of the stream
                            PdfBitmap image = new(memoryStream);
                            graphics.DrawImage(image, new Syncfusion.Drawing.PointF(193, 250)); // Coordinate for the cropped image

                        }
                        using MemoryStream pdfStream = new();
                        doc.Save(pdfStream);
                        resultList.Add(excelKey, pdfStream.ToArray());
                    }
                }

                return new SuccessResult(resultList);
            }
            catch (Exception ex)
            {
                    throw new Exception(ex.Message);
            }
        }
        public static Result<object> CropAndResizeMultiple(IEnumerable<IFormFile> images, int width, int height, int x, int y)
        {
            try
            {
                var processedImages = new Dictionary<string, byte[]>();

                foreach (var img in images)
                {
                    string imageExtension = Path.GetExtension(img.FileName).ToLower();

                    if (img == null || img.Length == 0)
                        return new ErrorResult($"{img?.FileName} is an invalid image file.");
                    if (img.FileName.Length < 24)
                        return new ErrorResult($"{img?.FileName} is not a supported image file.");
                    if (!(imageExtension == ".jpg" || imageExtension == ".png" || imageExtension == ".jpeg"))
                    {
                        return new ErrorResult($"{img?.FileName} is an invalid PDF.");
                    }

                    using System.Drawing.Image originalImage = System.Drawing.Image.FromStream(img.OpenReadStream());
                    // Crop the image
                    using Bitmap croppedBitmap = new Bitmap(width, height);
                    croppedBitmap.SetResolution(originalImage.HorizontalResolution, originalImage.VerticalResolution);

                    using Graphics graphic = Graphics.FromImage(croppedBitmap);
                    graphic.SmoothingMode = SmoothingMode.AntiAlias;
                    graphic.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    graphic.PixelOffsetMode = PixelOffsetMode.HighQuality;

                    graphic.DrawImage(originalImage, new System.Drawing.Rectangle(0, 0, width, height), x, y, width, height, System.Drawing.GraphicsUnit.Pixel);

                    // Resize the cropped image
                    using Bitmap resizedBitmap = new Bitmap(300, 300);
                    using Graphics resizedGraphic = Graphics.FromImage(resizedBitmap);
                    resizedGraphic.SmoothingMode = SmoothingMode.AntiAlias;
                    resizedGraphic.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    resizedGraphic.PixelOffsetMode = PixelOffsetMode.HighQuality;

                    resizedGraphic.DrawImage(croppedBitmap, 0, 0, 300, 300);

                    // Convert resized image to byte array
                    using MemoryStream ms = new();
                    resizedBitmap.Save(ms, originalImage.RawFormat);
                    processedImages[img.FileName] = ms.ToArray();
                }

                return new SuccessResult(processedImages);
            }
            catch (Exception)
            {

                throw;
            }
        }
        private static Dictionary<string, string> ReadExcelFile(IFormFile excel)
        {
            try
            {
                var data = new Dictionary<string, string>();
                ExcelPackage.LicenseContext = OfficeOpenXml.LicenseContext.Commercial;
                using (var package = new ExcelPackage(excel.OpenReadStream()))
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets[0];
                    int rowCount = worksheet.Dimension.Rows;
                    for (int row = 2; row <= rowCount; row++) // The first row contains headers, the count starts from row 2
                    {
                        string accountNumber = worksheet.Cells[row, 8].Value.ToString();
                        string merchantName = worksheet.Cells[row, 2].Value.ToString();
                        if (data.ContainsKey(merchantName))
                        {
                            data.Remove(merchantName);
                        }
                        data.Add(merchantName, accountNumber);
                    }

                }

                return data;
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
