using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using iText.Bouncycastle.Crypto;
using iText.Bouncycastle.X509;
using iText.Commons.Bouncycastle.Cert;
using iText.Kernel.Geom;
using iText.Kernel.Pdf;
using iText.Signatures;
using Org.BouncyCastle.Crypto;
using Org.BouncyCastle.Pkcs;
using Org.BouncyCastle.X509;


namespace MyWebAPI.Controllers
{
    [Route("api/itext")]
    [ApiController]
    public class DigitalSignatureController : ControllerBase
    {

        public static readonly string PASSWORD = "P@ssw0rd";
        public static readonly string KEYSTORE = AppContext.BaseDirectory + "Certificate\\certificate.p12";

        public DigitalSignatureController()
        {
            // LicenseKey.LoadLicenseFile(AppContext.BaseDirectory + "License/itextkey.xml");
        }

        public class signDigitalSignaturesRequest
        {
            public required IFormFile pdfFile { get; set; }
        }

        [HttpPost("sign_digital_signatures")]
        public async Task<IActionResult> signPdf([FromForm] signDigitalSignaturesRequest request)
        {
            if (request.pdfFile == null) throw new Exception("FILE CANNOT BE NULL");

            // Pre-allocate the MemoryStream
            using MemoryStream msSourceFile = new();
            await request.pdfFile.CopyToAsync(msSourceFile);

            // Reset the position to the beginning for further processing
            msSourceFile.Seek(0, SeekOrigin.Begin);

            PdfReader pdfStreamSrc = new PdfReader(msSourceFile);

            // Pre-allocate the destination MemoryStream with the same capacity as the source
            using MemoryStream memoryStream = new MemoryStream();
            PdfWriter pdfStreamDest = new PdfWriter(memoryStream);


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

            PdfSigner signer = new PdfSigner(pdfStreamSrc, pdfStreamDest, new StampingProperties());

            // Create the signature appearance
            Rectangle rect = new Rectangle(36, 648, 200, 100);
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

            pdfStreamDest.Close();
            pdfStreamSrc.Close();

            byte[] response = memoryStream.ToArray();
            return File(response, "application/pdf", "DigitalSignatureSigned.pdf");

        }




    }
}
