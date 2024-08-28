using iText.IO.Image;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using Microsoft.AspNetCore.Mvc;


namespace MyWebAPI.Controllers
{
    public class PdfController : ControllerBase
    {
        [HttpGet("create_pdf")]
        public async Task<IActionResult> createdPdf()
        {
            using MemoryStream memoryStream = new MemoryStream();
            PdfWriter pdfStreamDest = new PdfWriter(memoryStream);
            PdfDocument pdfDoc = new PdfDocument(pdfStreamDest);


            pdfDoc.SetTagged();
            PageSize pageSize = PageSize.A4.Rotate();
            pdfDoc.SetDefaultPageSize(pageSize);

            PdfDocumentInfo info = pdfDoc.GetDocumentInfo();
            info
            .SetTitle("title")
            .SetAuthor("auther")
            .SetSubject("Subject")
            .SetCreator("Creator")
            .SetKeywords("MeteData")
            .AddCreationDate();

            Document document = new Document(pdfDoc);
            Paragraph element = new Paragraph("Hellow World").SetFontSize(10);
            document.Add(element);

            // Adding image from file path
            string imagePath = AppContext.BaseDirectory + "Certificate\\images.jpg";
            ImageData imageData = ImageDataFactory.Create(imagePath);
            Image pdfImage = new Image(imageData);
            pdfImage.SetWidth(640); 
            pdfImage.SetHeight(480);
            // pdfImage.SetFixedPosition(100, 400);
            pdfImage.SetHorizontalAlignment(iText.Layout.Properties.HorizontalAlignment.CENTER);
            document.Add(pdfImage);

            pdfDoc.Close();

            byte[] response = memoryStream.ToArray();
            return await Task.FromResult<IActionResult>(File(response, "application/pdf", "pdf_file.pdf"));
        }
    }
}