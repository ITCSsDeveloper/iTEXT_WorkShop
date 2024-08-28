using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.IO.Image;
using iText.Kernel.Pdf.Extgstate;
using iText.Kernel.Pdf.Canvas;


using iText.Bouncycastle.Crypto;
using iText.Bouncycastle.X509;
using iText.Commons.Bouncycastle.Cert;
using iText.Signatures;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.X509;


namespace MyWebAPI.Controllers
{
    [Route("api/itext")]
    [ApiController]
    public class TestController : ControllerBase
    {
        public TestController()
        {
            // LicenseKey.LoadLicenseFile(AppContext.BaseDirectory + "License/itextkey.xml");
        }

        public static readonly string PASSWORD = "P@ssw0rd";
        public static readonly string KEYSTORE = AppContext.BaseDirectory + "Certificate\\certificate.p12";

        private static readonly string IMG_WATERMARK_PATH = AppContext.BaseDirectory + "Certificate\\images.jpg";
        private static readonly string PATH_SAVE_FILE = AppContext.BaseDirectory + "TEMP.pdf";


        [HttpGet("test")]
        public async Task<IActionResult> watermark( )
        {
            MemoryStream memoryStream = new MemoryStream();
            PdfWriter pdfStreamDest = new PdfWriter(memoryStream);
            PdfDocument pdfDoc = new PdfDocument(pdfStreamDest);

            // --- Set Info 
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

            Table table = new Table(5);
            table.SetWidth(UnitValue.CreatePercentValue(50));
            table.SetTextAlignment(TextAlignment.LEFT);

            // Add Header 
            table.AddCell(new Cell().Add(new Paragraph("Fname")));
            table.AddCell(new Cell().Add(new Paragraph("Lname")));
            table.AddCell(new Cell().Add(new Paragraph("Id")));
            table.AddCell(new Cell().Add(new Paragraph("Gender")));
            table.AddCell(new Cell().Add(new Paragraph("Province")));

            // Add New Row 
            table.StartNewRow();

            // Add Data or Loop
            table.AddCell(new Cell().Add(new Paragraph("Ratxxxnon")));
            table.AddCell(new Cell().Add(new Paragraph("Chxxxxoot")));
            table.AddCell(new Cell().Add(new Paragraph("60xxxx04")));
            table.AddCell(new Cell().Add(new Paragraph("M")));
            table.AddCell(new Cell().Add(new Paragraph("Roi-ET")));



            // Make Page Number 
            int numberOfPages = pdfDoc.GetNumberOfPages();
            for (int i = 1; i <= numberOfPages; i++)
            {
                float x = pageSize.GetRight() - 30;
                float y = pageSize.GetBottom() + 20;
                document.ShowTextAligned(new Paragraph(String.Format($"page {i} of {numberOfPages}")), x, y, i, TextAlignment.RIGHT, VerticalAlignment.TOP, 0);
            }


            // Make Watermark
            ImageData img = ImageDataFactory.Create(IMG_WATERMARK_PATH);
            float w = img.GetWidth();
            float h = img.GetHeight();

            PdfExtGState gs1 = new PdfExtGState().SetFillOpacity(0.5f);
            for (int i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
            {
                PdfPage pdfPage = pdfDoc.GetPage(i);
                Rectangle pSize = pdfPage.GetPageSizeWithRotation();

                pdfPage.SetIgnorePageRotationForContent(true);

                float x = (pSize.GetLeft() + pSize.GetRight()) / 2;
                float y = (pSize.GetTop() + pSize.GetBottom()) / 2;

                PdfCanvas over = new PdfCanvas(pdfDoc.GetPage(i));
                over.SaveState();
                over.SetExtGState(gs1);

                if (i % 2 == 1)
                {
                    over.AddImageWithTransformationMatrix(img, w, 0, 0, h, x - (w / 2), y - (h / 2), false);
                }
                else
                {
                    Paragraph paragraph = new Paragraph(""); // Dummy Text
                    document.ShowTextAligned(paragraph, x, y, i, TextAlignment.CENTER, VerticalAlignment.TOP, 45);
                }
                over.RestoreState();
            }
            document.Add(table);
            document.Close();
            pdfDoc.Close();

            // Write PDF to File 
            System.IO.File.WriteAllBytes(PATH_SAVE_FILE, memoryStream.ToArray());

            // Read File to Steam
            MemoryStream memoryStream2 = new MemoryStream();
            FileStream fileStream = new FileStream(PATH_SAVE_FILE, FileMode.Open, FileAccess.Read);
            await fileStream.CopyToAsync(memoryStream2);

            // Set Begin
            memoryStream2.Seek(0, SeekOrigin.Begin);
            PdfReader pdfStreamSrc = new PdfReader(memoryStream2);
            PdfWriter pdfStreamDestSig = new PdfWriter(memoryStream2);

            // Generate E Signature
            Pkcs12Store pk12 = new Pkcs12StoreBuilder().Build();
            pk12.Load(new FileStream(KEYSTORE, FileMode.Open, FileAccess.Read), PASSWORD.ToCharArray());
            string? alias = null;
            foreach (var a in pk12.Aliases)
            {
                alias = ((string)a);
                if (pk12.IsKeyEntry(alias))
                    break;
            }

            ICipherParameters pk = pk12.GetKey(alias).Key;
            X509CertificateEntry[] ce = pk12.GetCertificateChain(alias);
            X509Certificate[] chain = new X509Certificate[ce.Length];
            for (int k = 0; k < ce.Length; ++k)
            {
                chain[k] = ce[k].Certificate;
            }

            PdfSigner signer = new PdfSigner(pdfStreamSrc, pdfStreamDestSig, new StampingProperties());

            // Create the signature appearance
            Rectangle rect = new Rectangle(1, 1, 200, 100);
            signer
                .SetReason("Test digital signature signing.")
                .SetLocation("My Company")
                .SetPageRect(rect)
                .SetPageNumber(1);

            signer.SetFieldName("sig");

            IExternalSignature pks = new PrivateKeySignature(new PrivateKeyBC(pk), DigestAlgorithms.SHA256);

            IX509Certificate[] certificateWrappers = new IX509Certificate[chain.Length];
            for (int i = 0; i < certificateWrappers.Length; ++i)
            {
                certificateWrappers[i] = new X509CertificateBC(chain[i]);
            }

            // Sign the document using the detached mode, CMS or CAdES equivalent.
            signer.SignDetached(pks, certificateWrappers, null, null, null, 0, PdfSigner.CryptoStandard.CMS);

            // Return to File PDF
            var response = memoryStream2.ToArray();
            return await Task.FromResult<IActionResult>(File(response, "application/pdf", "DigitalSignatureSigned.pdf"));
        }
    }
}
