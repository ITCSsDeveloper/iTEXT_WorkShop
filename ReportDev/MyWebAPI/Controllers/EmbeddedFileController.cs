using iText.Kernel.Pdf;
using iText.Kernel.Pdf.Filespec;
using Microsoft.AspNetCore.Mvc;

namespace MyWebAPI.Controllers
{
    [Route("api/itext")]
    [ApiController]
    public class EmbeddedFileController : ControllerBase
    {

        public EmbeddedFileController()
        {
            // LicenseKey.LoadLicenseFile(AppContext.BaseDirectory + "License/itextkey.xml");
        }

        public class EmbeddedFileRequest
        {
            public required IFormFile pdfFile { get; set; }
            public required IFormFile embeddedFile { get; set; }
            public required string embeddedFileName { get; set; }
            public required string embeddedFileDescription { get; set; }
        }

        [HttpPost("embedded_file")]
        public async Task<IActionResult> embeddedFile([FromForm] EmbeddedFileRequest request)
        {

            string message = "";

            if (request.pdfFile == null) throw new Exception("pdfFile can't be null");
            if (request.embeddedFile == null) throw new Exception("embeddedFile can't be null");
            if (request.embeddedFileName == null) throw new Exception("embeddedFileName can't be null");
            if (request.embeddedFileDescription == null) throw new Exception("embeddedFileDescription can't be null");

            using MemoryStream msSourceFile = new MemoryStream();
            await request.pdfFile.CopyToAsync(msSourceFile);
            msSourceFile.Position = 0;
            PdfReader sourcePdf = new PdfReader(msSourceFile);

            using MemoryStream msDestinationFile = new MemoryStream();
            msDestinationFile.Position = 0;
            PdfWriter desticationPdf = new PdfWriter(msDestinationFile);

            PdfDocument pdfDocument = new PdfDocument(sourcePdf, desticationPdf);

            using MemoryStream msEmbeddedFile = new MemoryStream();
            await request.embeddedFile.CopyToAsync(msEmbeddedFile);

            string embeddedFileName = request.embeddedFileName + Path.GetExtension(request.embeddedFile.FileName) ;

            PdfFileSpec spec = PdfFileSpec.CreateEmbeddedFileSpec(pdfDocument, msEmbeddedFile.ToArray(), request.embeddedFileDescription, embeddedFileName, null, null, null);

            pdfDocument.AddFileAttachment("embedded_file", spec);

            pdfDocument.Close();
            sourcePdf.Close();
            desticationPdf.Close();

            byte[] response = msDestinationFile.ToArray();
            return File(response, "application/pdf", "Pdf_Embedded_file.pdf");
        }

    }
}
