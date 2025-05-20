using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using CommonLib.DataAccess;

using WebHome.Properties;
using MessagingToolkit.QRCode.Codec;
using ModelCore.DataEntity;
using ModelCore.Helper;
using ModelCore.Locale;

using CommonLib.Utility;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using CommonLib.Core.Utility;
using PdfSharp.Pdf;
using PdfSharp.Pdf.IO;
using QRCoder;
using WebHome.Controllers;
using ZXing;
using ZXing.Common;
using ZXing.QrCode;
using ZXing.QrCode.Internal;
using ZXing.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.Razor;
using System.Net.Mail;
using CommonLib.Core.Controllers;
using ZXing.Windows.Compatibility;


namespace WebHome.Helper
{
    public static partial class ExtensionMethods
    {
        public static int[] GetKeyValue(this String keyValue)
        {
            return keyValue.Split(',').Select(s => int.Parse(s)).ToArray();
        }

        public static String[] GetItemSelection(this HttpRequest request)
        {
            return request.Form["chkItem"];
        }

        public static void EnqueueInvoicePrint(this UserProfile userProfile, GenericManager<EIVOEntityDataContext> mgr, IEnumerable<int> docID)
        {
            foreach (var id in docID)
            {
                var item = mgr.GetTable<CDS_Document>().Where(i => i.DocID == id).FirstOrDefault();
                if (item != null && item.DocumentPrintQueue == null)
                {
                    item.DocumentPrintQueue = new DocumentPrintQueue
                    {
                        SubmitDate = DateTime.Now,
                        UID = userProfile.UID
                    };
                    mgr.SubmitChanges();
                }
            }
        }

        public static bool EnqueueDocumentPrint(this UserProfile userProfile, GenericManager<EIVOEntityDataContext> mgr, IEnumerable<int> docID)
        {
            bool result = false;
            foreach (var id in docID)
            {
                var item = mgr.GetTable<CDS_Document>().Where(i => i.DocID == id).FirstOrDefault();
                if (item != null && item.DocumentPrintQueue == null
                    && (item.InvoiceItem == null || item.IsPrintableInvoice()))
                {
                    item.DocumentPrintQueue = new DocumentPrintQueue
                    {
                        SubmitDate = DateTime.Now,
                        UID = userProfile.UID
                    };
                    result = true;
                }
            }
            if (result)
                mgr.SubmitChanges();
            return result;
        }

        public static bool EnqueueInvoicePrint(this UserProfile userProfile, GenericManager<EIVOEntityDataContext> mgr, IEnumerable<int> docID, out String reason)
        {
            bool result = false;
            reason = null;
            foreach (var id in docID)
            {
                var item = mgr.GetTable<CDS_Document>().Where(i => i.DocID == id).FirstOrDefault();
                if (item == null || item.InvoiceItem == null)
                    continue;
                if (item.InvoiceItem.Organization.OrganizationStatus.EntrustToPrint == false && item.DocumentPrintLog.Any() && item.DocumentAuthorization == null)
                {
                    reason = $"發票已列印({item.InvoiceItem.TrackCode}{item.InvoiceItem.No})，請取得授權列印!!";
                    return false;
                }
                if (item.DocumentPrintQueue == null)
                {
                    if (item.IsPrintableInvoice())
                    {
                        item.DocumentPrintQueue = new DocumentPrintQueue
                        {
                            SubmitDate = DateTime.Now,
                            UID = userProfile.UID
                        };
                        result = true;
                    }
                    else
                    {
                        reason = $"個人發票({item.InvoiceItem.TrackCode}{item.InvoiceItem.No})不提供自行列印，如有需要紙本請與管理部承辦人員連絡，謝謝配合。";
                        return false;
                    }
                }
                else
                {
                    return true;
                }
            }
            if (result)
                mgr.SubmitChanges();
            return result;
        }

        public static byte[] EncryptString(this String EncryptContent, byte[] key)
        {
            Aes aesCSP = Aes.Create();
            aesCSP.Key = key;
            aesCSP.GenerateIV();

            byte[] inBlock = Encoding.Default.GetBytes(EncryptContent);
            ICryptoTransform xfrm = aesCSP.CreateEncryptor();
            byte[] outBlock = xfrm.TransformFinalBlock(inBlock, 0, inBlock.Length);

            return outBlock;
        }

        public static bool ParseDate(this String dateStr, out DateTime? dateValue)
        {
            dateValue = null;
            if (!String.IsNullOrEmpty(dateStr))
            {
                DateTime dateVal;
                if (DateTime.TryParse(dateStr, out dateVal))
                {
                    dateValue = dateVal;
                    return true;
                }
            }
            return false;
        }

        public static bool IsPrintableInvoice(this CDS_Document item)
        {
            return item.InvoiceItem == null
                ? false
                : item.InvoiceItem.InvoiceBuyer.IsB2C()
                    ? item.InvoiceItem.PrintMark == "Y"
                        || (item.InvoiceItem.PrintMark == "N"
                                && item.InvoiceItem.InvoiceWinningNumber != null
                                && item.InvoiceItem.InvoiceDonation == null
                                && item.InvoiceItem.InvoiceCarrier != null
                                && item.InvoiceItem.InvoiceCarrier.CarrierType == ModelExtension.Properties.AppSettings.Default.DefaultUserCarrierType)
                        ? /*item.DocumentPrintLog.Any(l => l.TypeID == (int)Naming.DocumentTypeDefinition.E_Invoice)
                            ? item.DocumentAuthorization != null
                                ? true : false
                            :*/ true
                        : false
                    : /*item.InvoiceItem.Organization.OrganizationStatus.EntrustToPrint == false
                        ? item.DocumentPrintLog.Any(l => l.TypeID == (int)Naming.DocumentTypeDefinition.E_Invoice)
                            ? item.DocumentAuthorization != null ? true : false
                            : true
                        :*/ true;
        }

        public static bool IsPrintableInvoice(this InvoiceItem item)
        {
            return item == null
                ? false
                : item.InvoiceBuyer.IsB2C()
                    ? item.PrintMark == "Y"
                        || (item.PrintMark == "N"
                                && item.InvoiceWinningNumber != null
                                && item.InvoiceDonation == null
                                && item.InvoiceCarrier != null
                                && item.InvoiceCarrier.CarrierType == ModelExtension.Properties.AppSettings.Default.DefaultUserCarrierType)
                        ? true
                        : false
                    : true;
        }

        public static async Task<string> CreateContentAsPDFAsync<T>(this SampleController controller, String viewPath, T model, double timeOutInMinute, String[]? args = null)
        {
            String path = Startup.MapPath("~/temp");
            path.CheckStoredPath();

            Guid uniqueID = Guid.NewGuid();
            String saveTo = Path.Combine(path, String.Format("{0}.htm", uniqueID));
            String pdfFile = Path.Combine(path, String.Format("{0}.pdf", uniqueID));
            String tempHtml = Path.Combine(CommonLib.Core.Utility.FileLogger.Logger.LogDailyPath, String.Format("{0}.htm", uniqueID));

            using (StreamWriter sw = new StreamWriter(tempHtml))
            {
                sw.Write(await controller.RenderViewToStringAsync(viewPath, model));
                sw.Flush();
                sw.Close();
            }
            File.Move(tempHtml, saveTo);

            saveTo.ConvertHtmlToPDF(pdfFile, timeOutInMinute/*, args*/);

            if (File.Exists(pdfFile))
            {
                File.Delete(saveTo);

                var t = Task.Run(() =>
                {
                    bool checking = true;
                    while (checking)
                    {
                        try
                        {
                            using (var fs = File.OpenRead(pdfFile))
                            {
                                fs.Close();
                                checking = false;
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex);
                            Thread.Sleep(100);
                        }
                    }
                });

                t.Wait();

                return pdfFile;
            }

            return null;
        }

        public static bool AssertFile(this String pdfFile)
        {
            if (File.Exists(pdfFile))
            {
                var t = Task.Run(() =>
                {
                    bool checking = true;
                    while (checking)
                    {
                        try
                        {
                            using (var fs = File.OpenRead(pdfFile))
                            {
                                fs.Close();
                                checking = false;
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex);
                            Thread.Sleep(100);
                        }
                    }
                });

                t.Wait();
                return true;
            }

            return false;
        }

        public static String MergePDF(this String pdfOut,IEnumerable<String> pdfSource)
        {
            using (PdfDocument outPdf = new PdfDocument())
            {
                foreach(var source in pdfSource)
                {
                    using (PdfDocument pdf = PdfReader.Open(source, PdfDocumentOpenMode.Import))
                    {
                        CopyPages(pdf, outPdf);
                    }
                }
                outPdf.Save(pdfOut);
            }
            return pdfOut;
        }

        private static void CopyPages(PdfDocument from, PdfDocument to)
        {
            for (int i = 0; i < from.PageCount; i++)
            {
                to.AddPage(from.Pages[i]);
            }
        }

        public static bool CheckSystemCompany(this UserProfile profile)
        {
            return profile?.CurrentUserRole?.OrganizationCategory.CategoryID == (int)Naming.CategoryID.COMP_SYS;
        }

        public static void SendActivationNotice(this UserProfile userProfile)
        {
            userProfile.NotifyToActivate();
        }

        public static Bitmap CreateQRCodeOld(String content, QRCodeEncoder.ENCODE_MODE encoding, int scale, int version, QRCodeEncoder.ERROR_CORRECTION errorCorrect)
        {
            if (String.IsNullOrEmpty(content))
            {
                return null;
            }

            QRCodeEncoder qrCodeEncoder = new QRCodeEncoder();

            qrCodeEncoder.QRCodeScale = scale;
            qrCodeEncoder.QRCodeVersion = version;
            qrCodeEncoder.QRCodeErrorCorrect = errorCorrect;
            qrCodeEncoder.CharacterSet = "UTF-8";

            return qrCodeEncoder.Encode(content);

        }

        public static String GetLeftQRCodeImageSrc(this InvoiceItem item, int width = 100, int height = 100, int qrVersion = 10)
        {
            bool retry = false;
            String qrContent = null;
            try
            {
                qrContent = item.GetQRCodeContent();
                return qrContent.CreateQRCodeImageSrc(width: width, height: height, qrVersion: qrVersion);
            }
            catch (Exception ex)
            {
                retry = true;
                Logger.Error(ex);
                Logger.Warn($"產生發票QR Code失敗 => {item.InvoiceID},\r\n{qrContent}\r\n{ex}");
            }

            if (retry)
            {
                try
                {
                    qrContent = $"{qrContent.Substring(0, 88)}:1:1:1:品項過長，詳列於發票明細:1:1:";
                    return qrContent.CreateQRCodeImageSrc(width: width, height: height, qrVersion: qrVersion);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                    Logger.Warn($"產生發票QR Code失敗 => {item.InvoiceID},\r\n{qrContent}\r\n{ex}");
                }
            }

            return null;

        }

        public static String GetBarcode39ImageSrc(this String content, int height = 50, int margin = 0, int? wide = null, int? narrow = null)
        {
            using (Bitmap img = content.GetCode39(false, wide, narrow, height, margin))
            {
                return CreateBase64Content(img, 0);
            }
        }

        public static string CreateBase64Content(Bitmap img, float dpi = 600f)
        {
            using (MemoryStream buffer = new MemoryStream())
            {
                if (dpi > 0)
                {
                    img.SetResolution(dpi, dpi);
                }
                img.Save(buffer, ImageFormat.Png);
                StringBuilder sb = new StringBuilder("data:image/png;base64,");
                sb.Append(Convert.ToBase64String(buffer.ToArray()));
                return sb.ToString();
            }
        }

        public static String CreateQRCodeImageSrc(this String content, int width = 100, int height = 100, int margin = 0, int qrVersion = 10)
        {
            using (Bitmap img = content.CreateQRCode(width, height, margin, qrVersion))
            {
                return CreateBase64Content(img);
            }
        }

        public static Bitmap CreateQRCode(this String content, int width = 160, int height = 160, int margin = 0, int? qrVersion = 10)
        {
            if (String.IsNullOrEmpty(content))
            {
                return null;
            }

            //QRCodeWriter qrCode = new QRCodeWriter();
            //Dictionary<EncodeHintType, object> options = new Dictionary<EncodeHintType, object>();
            //options.Add(EncodeHintType.CHARACTER_SET, "UTF-8");
            //options.Add(EncodeHintType.ERROR_CORRECTION, "L");
            //options.Add(EncodeHintType.MARGIN, 0);

            //var result = qrCode.encode(content, BarcodeFormat.QR_CODE, 160, 160);
            //BarcodeWriter writer = new BarcodeWriter();
            //return writer.Write(result);

            IBarcodeWriter<Bitmap> writer = new BarcodeWriter<Bitmap>()
            {
                Format = BarcodeFormat.QR_CODE,
                Options = new QrCodeEncodingOptions()
                {
                    ErrorCorrection = ErrorCorrectionLevel.L,
                    Margin = margin,
                    Width = width,
                    Height = height,
                    QrVersion = qrVersion,
                    CharacterSet = "UTF-8"   // 少了這一行中文就亂碼了
                },
                Renderer = new BitmapRenderer()
                {
                    Background = Color.White,
                    Foreground = Color.Black,
                    //Width = width,
                    //Height = height,
                }
            };

            Bitmap picCode = writer.Write(content);
            picCode.SetResolution(600f, 600f);
            return picCode;

            //QRCodeEncoder qrCodeEncoder = new QRCodeEncoder();

            //qrCodeEncoder.QRCodeEncodeMode = encoding;
            //qrCodeEncoder.QRCodeScale = scale;
            //qrCodeEncoder.QRCodeVersion = version;
            //qrCodeEncoder.QRCodeErrorCorrect = errorCorrect;
            //qrCodeEncoder.CharacterSet = "UTF-8";

            //return qrCodeEncoder.Encode(content);

        }

        //public static Bitmap CreateQRCode(this String content, int width = 160, int height = 160, int margin = 0, int qrVersion = 10)
        //{
        //    //if (String.IsNullOrEmpty(content))
        //    //{
        //    //    return null;
        //    //}

        //    //QRCodeWriter qrCode = new QRCodeWriter();
        //    //Dictionary<EncodeHintType, object> options = new Dictionary<EncodeHintType, object>();
        //    //options.Add(EncodeHintType.CHARACTER_SET, "UTF-8");
        //    //options.Add(EncodeHintType.ERROR_CORRECTION, "L");
        //    //options.Add(EncodeHintType.MARGIN, 0);

        //    //var result = qrCode.encode(content, BarcodeFormat.QR_CODE, 160, 160);
        //    //BarcodeWriter writer = new BarcodeWriter();
        //    //return writer.Write(result);

        //    using (QRCodeData codeData = QRCodeGenerator.GenerateQrCode(content, QRCodeGenerator.ECCLevel.L))
        //    {
        //        using (QRCode qrCode = new QRCode(codeData))
        //        {
        //            Bitmap qrImg = qrCode.GetGraphic(width);
        //            qrImg.SetResolution(600f, 600f);
        //            return qrImg;
        //        }
        //    }

        //    //IBarcodeWriter writer = new BarcodeWriter()
        //    //{
        //    //    Format = BarcodeFormat.QR_CODE,
        //    //    Options = new QrCodeEncodingOptions()
        //    //    {
        //    //        ErrorCorrection = ErrorCorrectionLevel.L,
        //    //        Margin = margin,
        //    //        Width = width,
        //    //        Height = height,
        //    //        QrVersion = qrVersion,
        //    //        CharacterSet = "UTF-8"   // 少了這一行中文就亂碼了
        //    //    }
        //    //};

        //    //Bitmap picCode = writer.Write(content);
        //    //picCode.SetResolution(600f, 600f);
        //    //return picCode;

        //    //QRCodeEncoder qrCodeEncoder = new QRCodeEncoder();

        //    //qrCodeEncoder.QRCodeEncodeMode = encoding;
        //    //qrCodeEncoder.QRCodeScale = scale;
        //    //qrCodeEncoder.QRCodeVersion = version;
        //    //qrCodeEncoder.QRCodeErrorCorrect = errorCorrect;
        //    //qrCodeEncoder.CharacterSet = "UTF-8";

        //    //return qrCodeEncoder.Encode(content);

        //}

        public static Bitmap CreateBarCode(this String content, int width = 320, int height = 80, int margin = 0)
        {
            if (String.IsNullOrEmpty(content))
            {
                return null;
            }

            //return content.GetCode39(false, width, height: height, margin: margin);

            IBarcodeWriter<Bitmap> writer = new BarcodeWriter<Bitmap>()
            {
                Format = BarcodeFormat.CODE_39,
                Options = new EncodingOptions()
                {
                    PureBarcode = true,
                    Margin = margin,
                    //Width = width,
                    Height = height,
                },
                Renderer = new BitmapRenderer()
                {
                    Background = Color.White,
                    Foreground = Color.Black,
                    //Width = width,
                    //Height = height,
                }
            };

            Bitmap picCode = writer.Write(content);
            //picCode.SetResolution(600f, 600f);
            return picCode;

            //QRCodeEncoder qrCodeEncoder = new QRCodeEncoder();

            //qrCodeEncoder.QRCodeEncodeMode = encoding;
            //qrCodeEncoder.QRCodeScale = scale;
            //qrCodeEncoder.QRCodeVersion = version;
            //qrCodeEncoder.QRCodeErrorCorrect = errorCorrect;
            //qrCodeEncoder.CharacterSet = "UTF-8";

            //return qrCodeEncoder.Encode(content);

        }

        //public static String CreateQRCodeImageSrc(this String content, QRCodeEncoder.ENCODE_MODE encoding = QRCodeEncoder.ENCODE_MODE.BYTE, int scale = 4, int version = 8, QRCodeEncoder.ERROR_CORRECTION errorCorrect = QRCodeEncoder.ERROR_CORRECTION.L, float dpi = 600f)
        //{
        //    using (Bitmap img = content.CreateQRCode())
        //    {
        //        using (MemoryStream buffer = new MemoryStream())
        //        {
        //            img.SetResolution(dpi, dpi);
        //            img.Save(buffer, ImageFormat.Png);
        //            StringBuilder sb = new StringBuilder("data:image/png;base64,");
        //            sb.Append(Convert.ToBase64String(buffer.ToArray()));
        //            return sb.ToString();
        //        }
        //    }
        //}

        public static String GetQRCodeContent(this InvoiceItem item)
        {
            var buyer = item.InvoiceBuyer;

            string finalEncryData = item.BuildEncryptedData();

            StringBuilder sb = new StringBuilder();
            sb.Append(item.TrackCode + item.No);
            sb.Append(String.Format("{0:000}{1:00}{2:00}", item.InvoiceDate.Value.Year - 1911, item.InvoiceDate.Value.Month, item.InvoiceDate.Value.Day));
            sb.Append(item.RandomNo);
            sb.Append(String.Format("{0:X8}", (int)(item.InvoiceAmountType.SalesAmount ?? 0)));
            sb.Append(String.Format("{0:X8}", (int)item.InvoiceAmountType.TotalAmount.Value));
            sb.Append(buyer.IsB2C() ? "00000000" : buyer.ReceiptNo);
            sb.Append(item.InvoiceSeller != null ? item.InvoiceSeller.ReceiptNo : item.Organization.ReceiptNo);
            sb.Append(finalEncryData);
            sb.Append(":");
            sb.Append("**********");
            sb.Append(":");
            sb.Append(item.InvoiceDetails.Count);
            sb.Append(":");
            sb.Append(item.InvoiceDetails.Count);
            sb.Append(":");
            sb.Append(2);
            sb.Append(":");
            StringBuilder details = new StringBuilder();
            foreach (var p in item.InvoiceDetails)
            {
                //sb.Append(p.InvoiceProduct.Brief);
                //sb.Append(":");
                foreach (var pd in p.InvoiceProduct.InvoiceProductItem)
                {
                    if (!pd.Piece.Value.Equals(0))
                    {
                        details.Append(p.InvoiceProduct.Brief);
                        details.Append(":");
                        details.Append(String.Format("{0:#0}", pd.Piece));
                        details.Append(":");
                        details.Append(String.Format("{0:#0}", pd.UnitCost));
                        details.Append(":");
                    }
                }
            }
            sb.Append(Convert.ToBase64String(Encoding.Default.GetBytes(details.ToString())));

            return sb.ToString();
        }

        public static int InvoiceTypeToFormat(this int type)
        {
            switch (type)
            {
                //case (int)Naming.InvoiceTypeDefinition.三聯式:
                //    return (int)Naming.InvoiceTypeFormat.三聯式;
                //case (int)Naming.InvoiceTypeDefinition.二聯式:
                //    return (int)Naming.InvoiceTypeFormat.二聯式;
                //case (int)Naming.InvoiceTypeDefinition.二聯式收銀機:
                //    return (int)Naming.InvoiceTypeFormat.二聯式收銀機;
                //case (int)Naming.InvoiceTypeDefinition.特種稅額:
                //    return (int)Naming.InvoiceTypeFormat.特種稅額;
                //case (int)Naming.InvoiceTypeDefinition.電子計算機:
                //    return (int)Naming.InvoiceTypeFormat.電子計算機;
                //case (int)Naming.InvoiceTypeDefinition.三聯式收銀機:
                //    return (int)Naming.InvoiceTypeFormat.三聯式收銀機;
                case (int)Naming.InvoiceTypeDefinition.一般稅額計算之電子發票:
                    return (int)Naming.InvoiceTypeFormat.一般稅額計算之電子發票;
                case (int)Naming.InvoiceTypeDefinition.特種稅額計算之電子發票:
                    return (int)Naming.InvoiceTypeFormat.特種稅額;
                default:
                    return (int)Naming.InvoiceTypeFormat.一般稅額計算之電子發票;
            }

        }

        public static String GetBarcode20ImageSrc(this String content, int height = 55, int margin = 0, int? wide = null, int? narrow = null) //Amy-1121113
        {
            using (Bitmap img = content.GetCode20(false, wide, narrow, height, margin))
            {
                return CreateBase64Content(img, 0);
            }
        }

        public static void ImportHtmlBody(this MailMessage message, String htmlBody)
        {
            HtmlAgilityPack.HtmlDocument doc = new HtmlAgilityPack.HtmlDocument();
            doc.LoadHtml(htmlBody);

            var items = doc.DocumentNode.SelectNodes("//img");
            if (items != null)
            {
                foreach (var imgNode in items)
                {
                    if (imgNode.Attributes["src"] == null || String.IsNullOrEmpty(imgNode.Attributes["src"].Value))
                        continue;
                    System.Net.Mail.Attachment item = new System.Net.Mail.Attachment(Startup.MapPath(imgNode.Attributes["src"].Value));
                    item.NameEncoding = Encoding.UTF8;
                    item.ContentId = Guid.NewGuid().ToString();
                    item.ContentDisposition.Inline = true;
                    imgNode.SetAttributeValue("src", "cid:" + item.ContentId);
                    message.Attachments.Add(item);
                }
            }

            message.BodyEncoding = Encoding.UTF8;
            message.Body = doc.DocumentNode.OuterHtml;

        }

        //Amy-1121113
        public static Bitmap GetCode20(this string strSource, bool printCode, int? wide = null, int? narrow = null, int? height = null, int? margin = null)
        {
            using (var ms = new MemoryStream())
            {
                strSource = (strSource ?? "").PadLeft(20);
                string postdata = $"{strSource.Substring(0, 6)}  {strSource.Substring(6, 6)}  {strSource.Substring(12, 2)}  {strSource.Substring(14, 5)}  {strSource.Substring(19, 1)}";
                string Postno = $"{strSource.Substring(0, 6)}";

                var writer = new BarcodeWriter<Bitmap>() 
                { 
                    Format = BarcodeFormat.CODE_128 ,
                    Renderer = new BitmapRenderer()
                    {
                        Background = Color.White,
                        Foreground = Color.Black,
                        //Width = width,
                        //Height = height,
                    }
                };//宣告 Code128條碼 

                // 設定 Narrow 和 Wide 的寬度
                var qrCodeOptions = new ZXing.QrCode.QrCodeEncodingOptions
                {
                    Width = narrow ?? (int)Math.Round(0.33 * 10, 1), // 窄條寬度，单位：0.01mm
                    Height = wide ?? (int)Math.Round(1.5 * 10, 1), // 寬條寬度，单位：0.01mm
                    PureBarcode = true // 不顯示文字資訊
                };
                writer.Options = qrCodeOptions; // 將 QrCodeEncodingOptions 賦給 BarcodeWriter

                // 調整條碼寬度、條碼高度等參數
                var options = new ZXing.Common.EncodingOptions
                {
                    Width = 55 * 10,              // 條碼寬度
                    Height = height ?? 120 * 10,  // 條碼高度
                    Margin = margin ?? 0,
                    PureBarcode = true            // 不顯示文字資訊
                };
                writer.Options = options;    // 將新的 options 賦給 BarcodeWriter
                System.Drawing.Image barcodeBitmap = writer.Write(strSource); //產生條形碼圖像

                // 計算空白區域的寬度和高度
                int clearAreaWidth = (int)Math.Round(3.0 * 10, 1);   // 將3.0mm轉換為0.01mm單位
                int clearAreaHeight = (int)Math.Round(3.0 * 10, 1);  // 將3.0mm轉換為0.01mm單位

                // 創建最終的 Bitmap，包含空白區域
                Bitmap objBitmap = new Bitmap(barcodeBitmap.Width + 2 * clearAreaWidth, barcodeBitmap.Height + 2 * clearAreaHeight);

                using (Graphics graphics = Graphics.FromImage(objBitmap))
                {
                    graphics.FillRectangle(Brushes.White, 0, 0, objBitmap.Width, objBitmap.Height); //設定Barcard及data Block的背景色
                    graphics.DrawImage(barcodeBitmap, clearAreaWidth, clearAreaHeight);             //在新圖像上繪制條形碼，包括空白區域
                    Brush brush = new SolidBrush(Color.Black);  //線條
                                                                //字型及大小
                    Font font = new Font("Meiryo UI", 25, FontStyle.Regular, GraphicsUnit.Pixel);
                    // 在新圖像上繪制 Postno 文字
                    SizeF textSizePostno = graphics.MeasureString(Postno, font);
                    PointF textLocationPostno = new PointF((objBitmap.Width - textSizePostno.Width - 60) / 2, -0f * textSizePostno.Height);  //文字向左移60Piex,上移0倍文字高度
                    graphics.DrawString("第 " + Postno + " 號", font, brush, textLocationPostno);
                    // 在新圖像上繪制 postcode 文字
                    SizeF textSizePostcode = graphics.MeasureString(postdata, font);
                    PointF textLocationPostcode = new PointF((objBitmap.Width - textSizePostcode.Width) / 2, objBitmap.Height - textSizePostcode.Height + 0.2f); //Barcode向下移0.2mm
                    graphics.DrawString(postdata, font, brush, textLocationPostcode);
                }

                return objBitmap;
            }
        }
    }

    public enum QueryType
    {
        電子發票,
        電子折讓單,
        作廢電子發票,
        作廢電子折讓單,
        中獎發票
    }
}