using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using iText.Commons.Bouncycastle.Cert;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Signatures;
using iText.Kernel.Pdf.Annot;
using iText.Layout;
using iText.Kernel.Font;
using iText.IO.Font.Constants;
using iText.Layout.Element;
using iText.IO.Image;
using iText.Kernel.Pdf.Extgstate;
using iText.Kernel.Pdf.Canvas;
using iText.Layout.Properties;


namespace MyWebAPI.Controllers
{
    [Route("api/itext")]
    [ApiController]
    public class WatermarkController : ControllerBase
    {
        public WatermarkController()
        {
            // LicenseKey.LoadLicenseFile(AppContext.BaseDirectory + "License/itextkey.xml");
        }

        public class WatermarkRequest
        {
            public required IFormFile pdfFile { get; set; }
            public required IFormFile imageWatermark { get; set; }
            public required string textWatermark { get; set; }
        }

        [HttpPost("watermark")]
        public async Task<IActionResult> watermark([FromForm] WatermarkRequest request)
        {
            if (request.pdfFile == null) throw new Exception("pdfFile can't be null");
            if (request.imageWatermark == null) throw new Exception("imageWatermark can't be null");
            if (request.textWatermark == null) throw new Exception("textWatermark can't be null");


            using MemoryStream msSourceFile = new MemoryStream();
            await request.pdfFile.CopyToAsync(msSourceFile);
            msSourceFile.Seek(0, SeekOrigin.Begin);
            PdfReader pdfStreamSrc = new PdfReader(msSourceFile);

            MemoryStream memoryStream = new MemoryStream();
            PdfWriter pdfStreamDest = new PdfWriter(memoryStream);

            PdfDocument pdfDoc = new PdfDocument(pdfStreamSrc, pdfStreamDest);
            Document doc = new Document(pdfDoc);

            PdfFont font = PdfFontFactory.CreateFont(StandardFonts.COURIER);
            Paragraph paragraph = new Paragraph(request.textWatermark)
                .SetFont(font)
                .SetFontSize(30);

            MemoryStream msImage = new MemoryStream();
            await request.imageWatermark.CopyToAsync(msImage);
            ImageData img = ImageDataFactory.Create(msImage.ToArray());
            float w = img.GetWidth();
            float h = img.GetHeight();

            PdfExtGState gs1 = new PdfExtGState().SetFillOpacity(0.5f);

            for (int i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
            {

                PdfPage pdfPage = pdfDoc.GetPage(i);
                Rectangle pageSize = pdfPage.GetPageSizeWithRotation();

                pdfPage.SetIgnorePageRotationForContent(true);

                float x = (pageSize.GetLeft() + pageSize.GetRight()) / 2;
                float y = (pageSize.GetTop() + pageSize.GetBottom()) / 2;

                PdfCanvas over = new PdfCanvas(pdfDoc.GetPage(i));
                over.SaveState();
                over.SetExtGState(gs1);

                if (i % 2 == 1)
                {
                    over.AddImageWithTransformationMatrix(img, w, 0, 0, h, x - (w / 2), y - (h / 2), false);
                }
                else
                {
                    doc.ShowTextAligned(paragraph, x, y, i, TextAlignment.CENTER, VerticalAlignment.TOP, 45);
                }
                over.RestoreState();
            }


          













            doc.Close();
            pdfDoc.Close();
            pdfStreamDest.Close();

            byte[] response = memoryStream.ToArray();
            return File(response, "application/pdf", "watermark_file.pdf");

            return BadRequest();
        }
    }
}
