using iText.Html2pdf;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using Microsoft.AspNetCore.Mvc;


namespace MyWebAPI.Controllers
{
    public class PdfHtmlController : ControllerBase
    {
        [HttpGet("create_pdfhtml")]
        public async Task<IActionResult> createPdfHtml()
        {
            Stream htmlSrc;
            var url = new Uri("https://raw.githubusercontent.com/itext/i7js-examples/develop/src/main/resources/htmlsamples/html/sxsw.html");
            using (HttpClient client = new HttpClient())
            {
                HttpResponseMessage httpResponse = await client.GetAsync(url);
                httpResponse.EnsureSuccessStatusCode();
                htmlSrc = await httpResponse.Content.ReadAsStreamAsync();

                using MemoryStream memoryStream = new MemoryStream();
                PdfWriter pdfStreamDest = new PdfWriter(memoryStream);
                PdfDocument pdfDoc = new PdfDocument(pdfStreamDest);

                pdfDoc.SetTagged();
                PageSize pageSize = PageSize.A4.Rotate();
                pdfDoc.SetDefaultPageSize(pageSize);
                HtmlConverter.ConvertToPdf(htmlSrc, pdfDoc);








                

                byte[] response = memoryStream.ToArray();
                pdfDoc.Close();
                pdfStreamDest.Close();
                memoryStream.Close();

                return File(response, "application/pdf", "PdfHtml_file.pdf");
            }
        }
    }
}