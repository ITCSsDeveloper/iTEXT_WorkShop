using iText.Kernel.Pdf;
using Microsoft.AspNetCore.Mvc;
using System.Text;
 
namespace MyWebAPI.Controllers
{
    [Route("api/itext")]
    [ApiController]
    public class EncryptionController : ControllerBase
    {
        public EncryptionController()
        {
            // LicenseKey.LoadLicenseFile(AppContext.BaseDirectory + "License/itextkey.xml");
        }
 
        public class EncryptionRequest
        {
            /// <summary>
            /// PDF File
            /// </summary>
            public required IFormFile pdfFile { get; set; }
 
            /// <summary>
            ///  Owner password
            /// </summary>
            public required string ownerPassword { get; set; }
 
            /// <summary>
            ///  User password
            /// </summary>
            public required string userPassword { get; set; }
        }
 
        /// <summary>
        /// Protect PDF with password.
        /// </summary>
        [HttpPost("encryption")]
        public async Task<IActionResult> encryptionAsync([FromForm] EncryptionRequest request)
        {
            var message = string.Empty;
 
            // try
            // {
                if (request.pdfFile == null) throw new Exception("FILE CANNOT BE NULL");
 
                // Pre-allocate the MemoryStream
                using MemoryStream msSourceFile = new();
                await request.pdfFile.CopyToAsync(msSourceFile);
 
                // Reset the position to the beginning for further processing
                msSourceFile.Seek(0, SeekOrigin.Begin);
 
                PdfReader reader = new PdfReader(msSourceFile);
 
                WriterProperties props = new WriterProperties()
                    .SetStandardEncryption(
                        Encoding.ASCII.GetBytes(request.userPassword),
                        Encoding.ASCII.GetBytes(request.ownerPassword),
                        
                        EncryptionConstants.ALLOW_PRINTING |
                        EncryptionConstants.ALLOW_COPY |
                        EncryptionConstants.ALLOW_SCREENREADERS,
 
                        EncryptionConstants.ENCRYPTION_AES_256 |
                        EncryptionConstants.DO_NOT_ENCRYPT_METADATA
                    );
 
                using MemoryStream memoryStream = new MemoryStream();
                PdfWriter writer = new PdfWriter(memoryStream, props);
                PdfDocument pdfDoc = new PdfDocument(reader, writer);
 
                pdfDoc.Close();
                writer.Close();
 
                byte[] response = memoryStream.ToArray();
                return File(response, "application/pdf", "Encryption.pdf");
 
            // }
            // catch (Exception ex)
            // {
                // message = ex.Message;
            // }
 
            // return BadRequest();
        }
 
    }
}