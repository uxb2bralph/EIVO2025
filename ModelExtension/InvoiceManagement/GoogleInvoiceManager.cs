using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;



using ModelCore.DataEntity;
using ModelCore.Helper;
using ModelCore.InvoiceManagement.enUS;
using ModelCore.Locale;
using ModelCore.Properties;
using ModelCore.Schema.EIVO;
using CommonLib.Utility;
using CommonLib.DataAccess;
using CommonLib.Core.Utility;

namespace ModelCore.InvoiceManagement
{
    public partial class GoogleInvoiceManager : InvoiceManagerV2
    {
        public static string AttachmentPoolPath
        {
            get;
            private set;
        }

        static GoogleInvoiceManager()
        {
            AttachmentPoolPath = Path.Combine(Logger.LogPath, ModelExtension.Properties.AppSettings.Default.UploadedFilePath);

            if (!Directory.Exists(AttachmentPoolPath))
                Directory.CreateDirectory(AttachmentPoolPath);
        }


        public GoogleInvoiceManager()
            : base()
        {

        }
        public GoogleInvoiceManager(GenericManager<EIVOEntityDataContext> mgr)
            : base(mgr)
        {

        }

        public override Dictionary<int, Exception> SaveUploadInvoiceAutoTrackNo(InvoiceRoot item, OrganizationToken owner)
        {
            Dictionary<int, Exception> result = new Dictionary<int, Exception>();
            TrackNoManager trackNoMgr = new TrackNoManager(this, owner.CompanyID);

            if (item != null && item.Invoice != null && item.Invoice.Length > 0)
            {
                //Organization donatory = owner.Organization.InvoiceWelfareAgencies.Select(w => w.WelfareAgency.Organization).FirstOrDefault();

                EventItems = null;
                List<InvoiceItem> eventItems = new List<InvoiceItem>();

                for (int idx = 0; idx < item.Invoice.Length; idx++)
                {
                    try
                    {
                        var invItem = item.Invoice[idx];

                        Exception ex;
                        Organization seller;
                        InvoicePurchaseOrder order;



                        if ((ex = invItem.CheckBusiness(this, owner, out seller)) != null)
                        {
                            result.Add(idx, ex);
                            continue;
                        }

                        if ((ex = invItem.CheckDataNumber(seller,this, out order)) != null)
                        {
                            result.Add(idx, ex);
                            continue;
                        }

                        if ((ex = invItem.CheckAmount()) != null)
                        {
                            result.Add(idx, ex);
                            continue;
                        }

                        #region 列印、捐贈、載具

                        bool all_printed = seller.OrganizationStatus.PrintAll == true;
                        bool print_mark = invItem.PrintMark == "y" || invItem.PrintMark == "Y";

                        InvoiceDonation donation;
                        if ((ex = invItem.CheckInvoiceDonation(seller, all_printed, print_mark, out donation)) != null)
                        {
                            result.Add(idx, ex);
                            continue;
                        }

                        InvoiceCarrier carrier;

                        if ((ex = invItem.CheckInvoiceCarrier(seller, donation, all_printed, print_mark, out carrier)) != null)
                        {
                            result.Add(idx, ex);
                            continue;
                        }

                        if ((ex = invItem.CheckMandatoryFields(seller, all_printed, print_mark)) != null)
                        {
                            result.Add(idx, ex);
                            continue;
                        }

                        #endregion

                        IEnumerable<InvoiceProductItem> productItems;
                        if ((ex = invItem.CheckInvoiceProductItems(out productItems)) != null)
                        {
                            result.Add(idx, ex);
                            continue;
                        }

                        InvoiceItem newItem = createInvoiceItem(owner, invItem, seller, order, print_mark, all_printed, carrier, donation, productItems);

                        if (!trackNoMgr.CheckInvoiceNo(newItem))
                        {
                            result.Add(idx, new Exception("Not Set invoice word has run track or invoice number"));
                            continue;
                        }
                        else
                        {
                            newItem.InvoiceDate = DateTime.Now;
                        }

                        this.EntityList.InsertOnSubmit(newItem);
                        checkAttachmentFromPool(order);

                        this.SubmitChanges();

                        eventItems.Add(newItem);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex);
                        result.Add(idx, ex);
                    }
                }

                if (eventItems.Count > 0)
                {
                    HasItem = true;
                }

                EventItems = eventItems;
            }

            trackNoMgr.Dispose();
            return result;
        }

        private void checkAttachmentFromPool(InvoicePurchaseOrder order)
        {
            //發票附件檢查

            #region 抓取暫存資料夾內檔案名稱

            var fileList = Directory.GetFiles(AttachmentPoolPath,String.Format("{0}*.*",order.OrderNo));
            if (fileList.Length > 0)
            {
                Dictionary<String, String> fileItems = new Dictionary<string, string>();

                //取得暫存資料夾底下檔案名稱
                foreach (var tempFile in fileList)
                {
                    String fileName = Path.GetFileName(tempFile);
                    String storedPath = Path.Combine(Logger.LogDailyPath, fileName);

                    fileItems.Add(tempFile, storedPath);
                    String keyName = Path.GetFileNameWithoutExtension(fileName);

                    order.InvoiceItem.CDS_Document
                        .Attachment.Add(new Attachment
                        {
                            KeyName = keyName,
                            StoredPath = storedPath
                        });
                }

                ThreadPool.QueueUserWorkItem(stateInfo =>
                    {
                        foreach (var item in fileItems)
                        {
                            if (File.Exists(item.Value))
                            {
                                File.Delete(item.Value);
                            }
                            File.Move(item.Key, item.Value);
                        }
                    });
            }
            #endregion
        }

    }
}
