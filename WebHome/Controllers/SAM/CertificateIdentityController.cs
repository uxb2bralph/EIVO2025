using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using System.Xml;

using WebHome.Helper;
using WebHome.Models.ViewModel;
using ModelCore.Models.ViewModel;

using WebHome.Properties;
using ModelCore.DataEntity;
using ModelCore.DocumentManagement;
using ModelCore.Helper;
using ModelCore.InvoiceManagement;
using ModelCore.Locale;
using ModelCore.Schema.EIVO.B2B;
using ModelCore.Schema.TurnKey;
using ModelCore.Schema.TXN;

using CommonLib.Utility;
using CommonLib.Security.UseCrypto;
using Newtonsoft.Json;
using System.Data;
using ModelCore.Helper;
using CommonLib.DataAccess;
using System.Security.Permissions;
using System.Security.Cryptography.X509Certificates;
using System.Security;

namespace WebHome.Controllers.SAM
{
    public class CertificateIdentityController : SampleController<InvoiceItem>
    {
        public CertificateIdentityController(IServiceProvider serviceProvider) : base(serviceProvider)
        {
        }

        // GET: SystemExceptionLog
        public ActionResult OrganizationCertificate(OrganizationViewModel viewModel)
        {
            ViewBag.ViewModel = viewModel;
            if (viewModel.KeyID != null)
            {
                viewModel.CompanyID = viewModel.DecryptKeyValue();
            }

            Organization item = models.GetTable<Organization>()
                .Where(u => u.CompanyID == viewModel.CompanyID)
                .FirstOrDefault();

            if (item == null)
            {
                return Json(new { result = false, message = "資料錯誤!!" });
            }

            ViewBag.DataItem = item;

            return View("~/Views/SAM/CertificateIdentity/OrganizationCertificate.cshtml", item);
        }

        public async Task<ActionResult> CommitItemAsync(OrganizationCertificateViewModel viewModel)
        {
            if (viewModel.PfxFile == null)
            {
                return Json(new { result = false, message = "未選取檔案或檔案上傳失敗" });
            }

            var result = OrganizationCertificate(viewModel);

            Organization item = ViewBag.DataItem as Organization;
            if (item == null)
            {
                return result;
            }

            try
            {
                //KeyContainerPermission perm = new KeyContainerPermission(KeyContainerPermissionFlags.Open | KeyContainerPermissionFlags.Export);
                //perm.Assert();

                using var input = viewModel.PfxFile.OpenReadStream();
                using(MemoryStream ms = new MemoryStream())
                {
                    await input.CopyToAsync(ms);
                    var buf = ms.ToArray();
                    X509Certificate2 cert = new X509Certificate2(buf, viewModel.PIN, X509KeyStorageFlags.Exportable);

                    if (item.OrganizationToken == null)
                    {
                        item.OrganizationToken = new OrganizationToken { };
                    }

                    Guid keyID = Guid.NewGuid();
                    item.OrganizationToken.X509Certificate = Convert.ToBase64String(cert.RawData);
                    item.OrganizationToken.Thumbprint = cert.Thumbprint;
                    item.OrganizationToken.KeyID = keyID;
                    item.OrganizationToken.PKCS12 = Convert.ToBase64String(cert.Export(X509ContentType.Pkcs12, keyID.ToString().Substring(0, 8)));

                    models.SubmitChanges();
                    return Json(new { result = true, message = $"更新憑證金鑰:{item.OrganizationToken.KeyID}" });
                }
            }
            catch(Exception ex)
            {
                CommonLib.Core.Utility.FileLogger.Logger.Error(ex);
                return Json(new { result = false, message = ex.Message });
            }
            finally
            {
                //CodeAccessPermission.RevertAll();
            }
        }
    }
}