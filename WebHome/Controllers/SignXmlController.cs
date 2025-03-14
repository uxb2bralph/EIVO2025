using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Linq;
using System.Data.SqlClient;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading;
using System.Web;
using Microsoft.AspNetCore.Mvc;

using System.Xml;


using ClosedXML.Excel;
using WebHome.Helper;
using WebHome.Models;
using WebHome.Models.ViewModel;
using ModelCore.Models.ViewModel;
using WebHome.Properties;
using ModelCore.DataEntity;
using ModelCore.Helper;
using ModelCore.InvoiceManagement;
using ModelCore.Locale;

using ModelCore.Helper;
using CommonLib.Utility;
using CommonLib.DataAccess;
using System.Security.Cryptography.X509Certificates;
using CommonLib.Security.UseCrypto;

namespace WebHome.Controllers
{
    public class CertQueryViewModel
    {
        public StoreName? CertStoreName { get; set; }
        public StoreLocation? CertStoreLocation { get; set; }
        public String CertName { get; set; }
        public String Subject { get; set; }
        public String CspName { get; set; }
        public String StorePass { get; set; }
        public bool? WhiteSpace { get; set; }
    }

    public class SignXmlController : SampleController<InvoiceItem>
    {
        public SignXmlController(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        // GET: SignXml
        public ActionResult Index()
        {
            return View();
        }

        public ActionResult ListCertificate(CertQueryViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;
            if (viewModel.CertStoreLocation.HasValue && viewModel.CertStoreName.HasValue)
            {
                X509Store x509Store = new X509Store(viewModel.CertStoreName.Value, viewModel.CertStoreLocation.Value);
                x509Store.Open(OpenFlags.ReadOnly);
                var items = x509Store.Certificates.Cast<X509Certificate2>();
                x509Store.Close();
                return View("~/Views/SignXml/Module/CertificateOptions.ascx", items);
            }

            return Json(new { result = false,message = "not found!!" });

        }

        public ActionResult CreateSignature(OrganizationViewModel viewModel)
        {
            Response.ContentType = "text/xml";

            OrganizationToken item = null;
            Guid keyID;
            if (Guid.TryParse(viewModel.KeyID, out keyID))
            {
                item = models.GetTable<OrganizationToken>().Where(t => t.KeyID == keyID).FirstOrDefault();
            }

            if (item == null)
            {
                return Content("<root>未建立簽章憑證!!</root>");
            }

            X509Certificate2 cert = new X509Certificate2(Convert.FromBase64String(item.PKCS12), viewModel.KeyID.Substring(0, 8));
            XmlDocument docMsg = new XmlDocument();
            Request.Body.Position = 0;
            docMsg.Load(Request.Body);

            CryptoUtility.SignXmlSHA256(docMsg, null, null, cert, "");

            return Content(docMsg.OuterXml);
        }

        public ActionResult Sign(CertQueryViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;

            if (!viewModel.CertStoreLocation.HasValue)
            {
                return Json(new { result = false, message = "StoreLocation not found!!" });
            }

            if (!viewModel.CertStoreName.HasValue)
            {
                return Json(new { result = false, message = "StoreName not found!!" });
            }

            if (String.IsNullOrEmpty(viewModel.CertName))
            {
                return Json(new { result = false, message = "Signing certificate not found!!" });
            }

            var xmlFile = Request.Form.Files["XmlFile"];
            if (xmlFile == null)
            {
                return Json(new { result = false, message = "Xml file not found!!" });
            }
            viewModel.Subject = viewModel.Subject.GetEfficientString();

            XmlDocument docMsg = new XmlDocument();
            if (viewModel.WhiteSpace == true)
            {
                docMsg.PreserveWhitespace = true;
            }

            using var input = xmlFile.OpenReadStream();
            docMsg.Load(input);
            X509Store x509Store = new X509Store(viewModel.CertStoreName.Value, viewModel.CertStoreLocation.Value);
            x509Store.Open(OpenFlags.ReadOnly);
            X509Certificate2 cert = null;
            foreach (X509Certificate2 certificate in x509Store.Certificates)
            {
                if (certificate.Subject == viewModel.CertName || (viewModel.Subject!=null && certificate.Subject.IndexOf(viewModel.Subject) >= 0))
                {
                    if (certificate.HasPrivateKey)
                    {
                        //X509Certificate2 signCert = new X509Certificate2(certificate.Export(X509ContentType.Pfx, "111111"), "111111", X509KeyStorageFlags.Exportable);
                        cert = certificate;
                        break;
                    }
                }
            }
            x509Store.Close();

            if (cert == null)
                return Json(new { result = false, message = "Signing certificate not found!!" });

            CryptoUtility.SignXmlSHA256(docMsg, viewModel.CspName, viewModel.StorePass, cert, "");
            Response.Clear();
            Response.ContentType = "application/octet-stream";
            Response.Headers.Add("Content-Disposition", string.Format("attachment;filename={0}", "SignedContext.xml"));
            docMsg.Save(this.Response.Body);

            return new EmptyResult { };
        }

        public ActionResult Verify(CertQueryViewModel viewModel)
        {
            var xmlSigFile = Request.Form.Files["XmlSigFile"];
            if (xmlSigFile == null)
            {
                return Json(new { result = false, message = "Xml signature file not found!!" });
            }

            XmlDocument doc = new XmlDocument();
            if (viewModel.WhiteSpace == true)
            {
                doc.PreserveWhitespace = true;
            }
            doc.Load(xmlSigFile.OpenReadStream());
            CryptoUtility cryptoUtility = new CryptoUtility();
            if (cryptoUtility.VerifyXmlSignature(doc))
            {
                return Json(new { result = true, message = "successful!!" });
                //this.lblMsg.Text = cryptoUtility.CA_Log.Subject;
                //this.lblMsg.ForeColor = Color.Black;
                //this.lbViewCert.Visible = true;
                //this.ViewState["cert"] = (object)new X509Certificate((X509Certificate)cryptoUtility.SignerCertificate);
            }
            else
                return Json(new { result = false, message = "failed!!" });
        }


    }
}