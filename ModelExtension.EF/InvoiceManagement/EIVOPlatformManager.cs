using CommonLib.Core.DataWork;
using CommonLib.Core.Utility;
using CommonLib.Utility;
using DocumentFormat.OpenXml.InkML;
using Microsoft.EntityFrameworkCore;
using ModelCore.DataEntity;
using ModelCore.DocumentManagement;
using ModelCore.Helper;
using ModelCore.Locale;
using ModelCore.Properties;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security.Cryptography.Pkcs;
using System.Text;
using System.Threading;
using System.Xml;


namespace ModelCore.InvoiceManagement
{
    public class EIVOPlatformManager
    {
        public EIVOPlatformManager()
        {
        }

        //public void TransmitInvoice()
        //{
        //    using (InvoiceManager mgr = new InvoiceManager())
        //    {
        //        SaveToPlatform(mgr);
        //    }
        //}

        class _key
        {
            public int? a = (int?)null;
            public int? b = (int?)null;
        };

        //public void NotifyCounterpartBusiness()
        //{
        //    using (InvoiceManager mgr = new InvoiceManager())
        //    {
        //        var notify = mgr.GetTable<ReplicationNotification>();
        //        var items = notify.ToList();

        //        var toIssue = mgr.GetTable<CDS_Document>().Where(d => d.CurrentStep == (int)Naming.B2BInvoiceStepDefinition.待開立);

        //        var notifyToIssue = toIssue
        //                .Join(mgr.EntityList, d => d.DocID, i => i.InvoiceID, (d, i) => i)
        //                .Join(notify, t => t.InvoiceID, s => s.DocID, (t, s) => new _key { a = t.InvoiceSeller.SellerID, b = t.InvoiceBuyer.BuyerID })
        //                .Join(mgr.GetTable<BusinessRelationship>(), k => k, r => new _key { a = r.RelativeID, b = r.MasterID }, (k, r) => k.a)
        //            .Concat(toIssue
        //                .Join(mgr.GetTable<DerivedDocument>(), t => t.DocID, d => d.DocID, (t, d) => d)
        //                .Join(mgr.EntityList, d => d.SourceID, i => i.InvoiceID, (d, i) => i)
        //                .Join(notify, t => t.InvoiceID, s => s.DocID, (t, s) => new _key { a = t.InvoiceSeller.SellerID, b = t.InvoiceBuyer.BuyerID })
        //                .Join(mgr.GetTable<BusinessRelationship>(), k => k, r => new _key { a = r.RelativeID, b = r.MasterID }, (k, r) => k.a))
        //            .Concat(toIssue
        //                .Join(mgr.GetTable<CDS_Document>(), d => d.DocID, i => i.AllowanceID, (d, i) => i)
        //                .Join(notify, t => t.AllowanceID, s => s.DocID, (t, s) => new _key { a = t.InvoiceAllowanceSeller.SellerID, b = t.InvoiceAllowanceBuyer.BuyerID })
        //                .Join(mgr.GetTable<BusinessRelationship>(), k => k, r => new _key { a = r.MasterID, b = r.RelativeID }, (k, r) => k.b))
        //            .Concat(toIssue
        //                .Join(mgr.GetTable<DerivedDocument>(), t => t.DocID, d => d.DocID, (t, d) => d)
        //                .Join(mgr.GetTable<CDS_Document>(), d => d.SourceID, i => i.AllowanceID, (d, i) => i)
        //                .Join(notify, t => t.AllowanceID, s => s.DocID, (t, s) => new _key { a = t.InvoiceAllowanceSeller.SellerID, b = t.InvoiceAllowanceBuyer.BuyerID })
        //                .Join(mgr.GetTable<BusinessRelationship>(), k => k, r => new _key { a = r.MasterID, b = r.RelativeID }, (k, r) => k.b))
        //            .Concat(toIssue
        //                .Join(mgr.GetTable<ReceiptItem>(), d => d.DocID, i => i.ReceiptID, (d, i) => i)
        //                .Join(notify, t => t.ReceiptID, s => s.DocID, (t, s) => new _key { a = t.SellerID, b = t.BuyerID })
        //                .Join(mgr.GetTable<BusinessRelationship>(), k => k, r => new _key { a = r.RelativeID, b = r.MasterID }, (k, r) => k.a))
        //            .Concat(toIssue
        //                .Join(mgr.GetTable<DerivedDocument>(), t => t.DocID, d => d.DocID, (t, d) => d)
        //                .Join(mgr.GetTable<ReceiptItem>(), d => d.SourceID, i => i.ReceiptID, (d, i) => i)
        //                .Join(notify, t => t.ReceiptID, s => s.DocID, (t, s) => new _key { a = t.SellerID, b = t.BuyerID })
        //                .Join(mgr.GetTable<BusinessRelationship>(), k => k, r => new _key { a = r.RelativeID, b = r.MasterID }, (k, r) => k.a))
        //            .Distinct();

        //        var toReceive = mgr.GetTable<CDS_Document>().Where(d => d.CurrentStep == (int)Naming.B2BInvoiceStepDefinition.待接收);

        //        var notifyToReceive = toReceive
        //                .Join(mgr.EntityList, d => d.DocID, i => i.InvoiceID, (d, i) => i)
        //                .Join(notify, t => t.InvoiceID, s => s.DocID, (t, s) => new _key { a = t.InvoiceSeller.SellerID, b = t.InvoiceBuyer.BuyerID })
        //                .Join(mgr.GetTable<BusinessRelationship>(), k => k, r => new _key { a = r.MasterID, b = r.RelativeID }, (k, r) => k.b)
        //            .Concat(toReceive
        //                .Join(mgr.GetTable<DerivedDocument>(), t => t.DocID, d => d.DocID, (t, d) => d)
        //                .Join(mgr.EntityList, d => d.SourceID, i => i.InvoiceID, (d, i) => i)
        //                .Join(notify, t => t.InvoiceID, s => s.DocID, (t, s) => new _key { a = t.InvoiceSeller.SellerID, b = t.InvoiceBuyer.BuyerID })
        //                .Join(mgr.GetTable<BusinessRelationship>(), k => k, r => new _key { a = r.MasterID, b = r.RelativeID }, (k, r) => k.b))
        //            .Concat(toReceive
        //                .Join(mgr.GetTable<CDS_Document>(), d => d.DocID, i => i.AllowanceID, (d, i) => i)
        //                .Join(notify, t => t.AllowanceID, s => s.DocID, (t, s) => new _key { a = t.InvoiceAllowanceSeller.SellerID, b = t.InvoiceAllowanceBuyer.BuyerID })
        //                .Join(mgr.GetTable<BusinessRelationship>(), k => k, r => new _key { a = r.RelativeID, b = r.MasterID }, (k, r) => k.a))
        //            .Concat(toReceive
        //                .Join(mgr.GetTable<DerivedDocument>(), t => t.DocID, d => d.DocID, (t, d) => d)
        //                .Join(mgr.GetTable<CDS_Document>(), d => d.SourceID, i => i.AllowanceID, (d, i) => i)
        //                .Join(notify, t => t.AllowanceID, s => s.DocID, (t, s) => new _key { a = t.InvoiceAllowanceSeller.SellerID, b = t.InvoiceAllowanceBuyer.BuyerID })
        //                .Join(mgr.GetTable<BusinessRelationship>(), k => k, r => new _key { a = r.RelativeID, b = r.MasterID }, (k, r) => k.a))
        //            .Concat(toReceive
        //                .Join(mgr.GetTable<ReceiptItem>(), d => d.DocID, i => i.ReceiptID, (d, i) => i)
        //                .Join(notify, t => t.ReceiptID, s => s.DocID, (t, s) => new _key { a = t.SellerID, b = t.BuyerID })
        //                .Join(mgr.GetTable<BusinessRelationship>(), k => k, r => new _key { a = r.MasterID, b = r.RelativeID }, (k, r) => k.b))
        //            .Concat(toReceive
        //                .Join(mgr.GetTable<DerivedDocument>(), t => t.DocID, d => d.DocID, (t, d) => d)
        //                .Join(mgr.GetTable<ReceiptItem>(), d => d.SourceID, i => i.ReceiptID, (d, i) => i)
        //                .Join(notify, t => t.ReceiptID, s => s.DocID, (t, s) => new _key { a = t.SellerID, b = t.BuyerID })
        //                .Join(mgr.GetTable<BusinessRelationship>(), k => k, r => new _key { a = r.MasterID, b = r.RelativeID }, (k, r) => k.b))
        //            .Distinct();


        //        var org = mgr.GetTable<Company>();

        //        foreach (var businessID in notifyToIssue)
        //        {
        //            var item = org.Where(o => o.CompanyID == businessID).FirstOrDefault();
        //            if (item != null && (item.OrganizationStatus == null || item.OrganizationStatus.Entrusting != true))
        //            {
        //                EIVOPlatformFactory.NotifyToIssueItem(this, new EventArgs<Company> { Argument = item });
        //            }
        //        }

        //        foreach (var businessID in notifyToReceive)
        //        {
        //            var item = org.Where(o => o.CompanyID == businessID).FirstOrDefault();
        //            if (item != null && (item.OrganizationStatus == null || item.OrganizationStatus.Entrusting != true))
        //            {
        //                EIVOPlatformFactory.NotifyToReceiveItem(this, new EventArgs<Company> { Argument = item });
        //            }
        //        }

        //        mgr.ExecuteCommand("delete dbo.DocumentReplication");
        //        mgr.ExecuteCommand("delete dbo.DocumentDispatch");
        //        notify.DeleteAllOnSubmit(items);
        //        mgr.SubmitChanges();

        //    }

        //}

        //private void SaveToPlatform(InvoiceManager mgr)
        //{
        //    //Settings.Default.A1401Outbound.CheckStoredPath();
        //    //int invoiceCounter = Directory.GetFiles(Settings.Default.A1401Outbound).Length;
        //    //Settings.Default.B1401Outbound.CheckStoredPath();
        //    //int allowanceCounter = Directory.GetFiles(Settings.Default.B1401Outbound).Length;
        //    ModelExtension.Properties.AppSettings.Default.A0501Outbound.CheckStoredPath();
        //    int cancellationCounter = Directory.GetFiles(ModelExtension.Properties.AppSettings.Default.A0501Outbound).Length;
        //    ModelExtension.Properties.AppSettings.Default.B0501Outbound.CheckStoredPath();
        //    int allowanceCancellationCounter = Directory.GetFiles(ModelExtension.Properties.AppSettings.Default.B0501Outbound).Length;

        //    var items = mgr.GetTable<CDS_Document>().Where(d => d.CurrentStep == (int)Naming.InvoiceStepDefinition.待傳送);

        //    if (items.Count() > 0)
        //    {
        //        String fileName;
        //        foreach (var item in items)
        //        {
        //            try
        //            {
        //                switch ((Naming.DocumentTypeDefinition)item.DocType.Value)
        //                {
        //                    case Naming.DocumentTypeDefinition.E_Invoice:
        //                        //if (item.CDS_Document.InvoiceSeller.Company.OrganizationStatus != null && item.CDS_Document.InvoiceSeller.Company.OrganizationStatus.IronSteelIndustry == true)
        //                        //{
        //                        //    fileName = Path.Combine(Settings.Default.A1401Outbound, String.Format("A1401-{0:yyyyMMddHHmmssf}-{1:00000}.xml", DateTime.Now, invoiceCounter++));
        //                        //    item.CDS_Document.CreateA1401().ConvertToXml().Save(fileName);
        //                        //}
        //                        //else
        //                        {
        //                            fileName = Path.Combine(ModelExtension.Properties.AppSettings.Default.A0401Outbound, $"A0401-{DateTime.Now:yyyyMMddHHmmssf}-{item.CDS_Document.TrackCode}{item.CDS_Document.No}.xml");
        //                            item.CDS_Document.CreateB2BInvoiceMIG().ConvertToXml().Save(fileName);
        //                        }
        //                        break;
        //                    case Naming.DocumentTypeDefinition.E_Allowance:
        //                        //if (item.CDS_Document.InvoiceAllowanceSeller.Company.OrganizationStatus != null && item.CDS_Document.InvoiceAllowanceSeller.Company.OrganizationStatus.IronSteelIndustry == true)
        //                        //{
        //                        //    fileName = Path.Combine(Settings.Default.B1401Outbound, String.Format("B1401-{0:yyyyMMddHHmmssf}-{1:00000}.xml", DateTime.Now, allowanceCounter++));
        //                        //    item.CDS_Document.CreateB1401().ConvertToXml().Save(fileName);
        //                        //}
        //                        //else
        //                        {
        //                            fileName = Path.Combine(ModelExtension.Properties.AppSettings.Default.B0401Outbound, $"B0401-{DateTime.Now:yyyyMMddHHmmssf}-{item.CDS_Document.AllowanceNumber}.xml");
        //                            item.CDS_Document.CreateG0401().ConvertToXml().Save(fileName);
        //                        }
        //                        break;
        //                    case Naming.DocumentTypeDefinition.E_InvoiceCancellation:
        //                        fileName = Path.Combine(ModelExtension.Properties.AppSettings.Default.A0501Outbound, String.Format("A0501-{0:yyyyMMddHHmmssf}-{1:00000}.xml", DateTime.Now, cancellationCounter++));
        //                        item.DerivedDocument.ParentDocument.CDS_Document.CreateF0501().ConvertToXml().Save(fileName);
        //                        break;
        //                    case Naming.DocumentTypeDefinition.E_AllowanceCancellation:
        //                        fileName = Path.Combine(ModelExtension.Properties.AppSettings.Default.B0501Outbound, String.Format("B0501-{0:yyyyMMddHHmmssf}-{1:00000}.xml", DateTime.Now, allowanceCancellationCounter++));
        //                        item.DerivedDocument.ParentDocument.CDS_Document.CreateG0501().ConvertToXml().Save(fileName);
        //                        break;
        //                }

        //                transmit(mgr, item);
        //            }
        //            catch (Exception ex)
        //            {
        //                Logger.Error(ex);
        //            }
        //        }
        //    }
        //}

        //public void CommissionedToReceive()
        //{
        //    using (InvoiceManager mgr = new InvoiceManager())
        //    {
        //        var items = mgr.GetTable<CDS_Document>().Where(d => d.CurrentStep == (int)Naming.InvoiceStepDefinition.待接收);

        //        if (items.Count() > 0)
        //        {
        //            StringBuilder sb = new StringBuilder();
        //            foreach (var item in items)
        //            {

        //                try
        //                {
        //                    bool bSigned = false;
        //                    switch ((Naming.B2BInvoiceDocumentTypeDefinition)item.DocType.Value)
        //                    {
        //                        case Naming.B2BInvoiceDocumentTypeDefinition.電子發票:

        //                            if (item.CDS_Document.InvoiceBuyer.Company.OrganizationStatus.Entrusting == true)
        //                            {
        //                                sb.Clear();
        //                                if (item.CDS_Document.InvoiceBuyer.Company.IsEnterpriseGroupMember())
        //                                {
        //                                    var cert = (new B2BInvoiceManager(mgr)).PrepareSignerCertificate(item.CDS_Document.InvoiceBuyer.Company);
        //                                    if (cert != null)
        //                                    {
        //                                        bSigned = item.CDS_Document.SignAndCheckToReceiveInvoiceItem(cert, sb);
        //                                    }
        //                                }
        //                                else
        //                                {
        //                                    bSigned = item.CDS_Document.SignAndCheckToReceiveInvoiceItem(null, sb);
        //                                }
        //                                if (bSigned)
        //                                {
        //                                    EIVONotificationFactory.NotifyCommissionedToReceiveA0401(new DocumentQueryViewModel { DocID = item.DocID });
        //                                }
        //                            }
        //                            break;
        //                        case Naming.B2BInvoiceDocumentTypeDefinition.發票折讓:
        //                            if (item.CDS_Document.InvoiceAllowanceSeller.Company.OrganizationStatus.Entrusting == true)
        //                            {
        //                                sb.Clear();
        //                                if (item.CDS_Document.InvoiceAllowanceSeller.Company.IsEnterpriseGroupMember())
        //                                {
        //                                    var cert = (new B2BInvoiceManager(mgr)).PrepareSignerCertificate(item.CDS_Document.InvoiceAllowanceSeller.Company);
        //                                    if (cert != null)
        //                                    {
        //                                        bSigned = item.CDS_Document.SignAndCheckToReceiveInvoiceAllowance(cert, sb);
        //                                    }
        //                                }
        //                                else
        //                                {
        //                                    bSigned = item.CDS_Document.SignAndCheckToReceiveInvoiceAllowance(null, sb);
        //                                }
        //                                if (bSigned)
        //                                {
        //                                    var businessID = new DocumentQueryViewModel
        //                                    {
        //                                        MailToID = item.CDS_Document.InvoiceAllowanceBuyer.BuyerID,
        //                                        Seller = item.CDS_Document.InvoiceAllowanceSeller.Company,
        //                                        DocID = item.DocID
        //                                    };
        //                                    EIVONotificationFactory.NotifyCommissionedToReceive(this, new EventArgs<DocumentQueryViewModel> { Argument = businessID });
        //                                }
        //                            }
        //                            break;
        //                        case Naming.B2BInvoiceDocumentTypeDefinition.作廢發票:
        //                            if (item.DerivedDocument.ParentDocument.CDS_Document.InvoiceBuyer.Company.OrganizationStatus.Entrusting == true)
        //                            {
        //                                sb.Clear();
        //                                if (item.DerivedDocument.ParentDocument.CDS_Document.InvoiceBuyer.Company.IsEnterpriseGroupMember())
        //                                {
        //                                    var cert = (new B2BInvoiceManager(mgr)).PrepareSignerCertificate(item.DerivedDocument.ParentDocument.CDS_Document.InvoiceBuyer.Company);
        //                                    if (cert != null)
        //                                    {
        //                                        bSigned = item.DerivedDocument.ParentDocument.CDS_Document.SignAndCheckToReceiveInvoiceCancellation(cert, sb, item.DocID);
        //                                    }
        //                                }
        //                                else
        //                                {
        //                                    bSigned = item.DerivedDocument.ParentDocument.CDS_Document.SignAndCheckToReceiveInvoiceCancellation(null, sb, item.DocID);
        //                                }
        //                                if (bSigned)
        //                                {
        //                                    var businessID = new DocumentQueryViewModel
        //                                    {
        //                                        DocID = item.DocID
        //                                    };
        //                                    EIVONotificationFactory.NotifyCommissionedToReceiveInvoiceCancellation(this, new EventArgs<DocumentQueryViewModel> { Argument = businessID });
        //                                }
        //                            }
        //                            break;
        //                        case Naming.B2BInvoiceDocumentTypeDefinition.作廢折讓:
        //                            if (item.DerivedDocument.ParentDocument.CDS_Document.InvoiceAllowanceSeller.Company.OrganizationStatus.Entrusting == true)
        //                            {
        //                                sb.Clear();
        //                                if (item.DerivedDocument.ParentDocument.CDS_Document.InvoiceAllowanceSeller.Company.IsEnterpriseGroupMember())
        //                                {
        //                                    var cert = (new B2BInvoiceManager(mgr)).PrepareSignerCertificate(item.DerivedDocument.ParentDocument.CDS_Document.InvoiceAllowanceSeller.Company);
        //                                    if (cert != null)
        //                                    {
        //                                        bSigned = item.DerivedDocument.ParentDocument.CDS_Document.SignAndCheckToReceiveAllowanceCancellation(cert, sb, item.DocID);
        //                                    }
        //                                }
        //                                else
        //                                {
        //                                    bSigned = item.DerivedDocument.ParentDocument.CDS_Document.SignAndCheckToReceiveAllowanceCancellation(null, sb, item.DocID);
        //                                }
        //                                if (bSigned)
        //                                {
        //                                    var businessID = new DocumentQueryViewModel
        //                                    {
        //                                        MailToID = item.DerivedDocument.ParentDocument.CDS_Document.InvoiceAllowanceBuyer.BuyerID,
        //                                        Seller = item.DerivedDocument.ParentDocument.CDS_Document.InvoiceAllowanceSeller.Company,
        //                                        DocID = item.DocID
        //                                    };
        //                                    EIVONotificationFactory.NotifyCommissionedToReceive(this, new EventArgs<DocumentQueryViewModel> { Argument = businessID });
        //                                }
        //                            }
        //                            break;
        //                        case Naming.B2BInvoiceDocumentTypeDefinition.收據:
        //                            if (item.ReceiptItem != null && item.ReceiptItem.Buyer.OrganizationStatus.Entrusting == true)
        //                            {
        //                                sb.Clear();
        //                                if (item.ReceiptItem.Buyer.IsEnterpriseGroupMember())
        //                                {
        //                                    var cert = (new B2BInvoiceManager(mgr)).PrepareSignerCertificate(item.ReceiptItem.Buyer);
        //                                    if (cert != null)
        //                                    {
        //                                        bSigned = item.ReceiptItem.SignAndCheckToReceiveReceipt(cert, sb);
        //                                    }
        //                                }
        //                                else
        //                                {
        //                                    bSigned = item.ReceiptItem.SignAndCheckToReceiveReceipt(null, sb);
        //                                }
        //                                if (bSigned)
        //                                {
        //                                    var businessID = new DocumentQueryViewModel
        //                                    {
        //                                        MailToID = item.ReceiptItem.BuyerID,
        //                                        Seller = item.ReceiptItem.Seller,
        //                                        DocID = item.DocID
        //                                    };
        //                                    EIVONotificationFactory.NotifyCommissionedToReceive(this, new EventArgs<DocumentQueryViewModel> { Argument = businessID });
        //                                }
        //                            }
        //                            break;
        //                        case Naming.B2BInvoiceDocumentTypeDefinition.作廢收據:
        //                            if (item.DerivedDocument.ParentDocument.ReceiptItem.Buyer.OrganizationStatus.Entrusting == true)
        //                            {
        //                                sb.Clear();
        //                                if (item.DerivedDocument.ParentDocument.ReceiptItem.Buyer.IsEnterpriseGroupMember())
        //                                {
        //                                    var cert = (new B2BInvoiceManager(mgr)).PrepareSignerCertificate(item.DerivedDocument.ParentDocument.ReceiptItem.Buyer);
        //                                    if (cert != null)
        //                                    {
        //                                        bSigned = item.DerivedDocument.ParentDocument.ReceiptItem.SignAndCheckToReceiveReceiptCancellation(cert, sb, item.DocID);
        //                                    }
        //                                }
        //                                else
        //                                {
        //                                    bSigned = item.DerivedDocument.ParentDocument.ReceiptItem.SignAndCheckToReceiveReceiptCancellation(null, sb, item.DocID);
        //                                }
        //                                if (bSigned)
        //                                {
        //                                    var businessID = new DocumentQueryViewModel
        //                                    {
        //                                        MailToID = item.DerivedDocument.ParentDocument.ReceiptItem.BuyerID,
        //                                        Seller = item.DerivedDocument.ParentDocument.ReceiptItem.Seller,
        //                                        DocID = item.DocID
        //                                    };
        //                                    EIVONotificationFactory.NotifyCommissionedToReceive(this, new EventArgs<DocumentQueryViewModel> { Argument = businessID });
        //                                }
        //                            }
        //                            break;
        //                        default:
        //                            break;
        //                    }

        //                    if (bSigned)
        //                    {
        //                        transmit(mgr, item);
        //                    }
        //                }
        //                catch (Exception ex)
        //                {
        //                    Logger.Error(ex);
        //                }
        //            }
        //        }
        //    }
        //}

        public void CommissionedToIssue()
        {
            using (InvoiceManager mgr = new InvoiceManager())
            {
                var items = mgr.GetTable<CDS_Document>().Where(d => d.CurrentStep == (int)Naming.InvoiceStepDefinition.待開立);

                if (items.Count() > 0)
                {
                    StringBuilder sb = new StringBuilder();
                    foreach (var item in items)
                    {
                        try
                        {
                            bool bSigned = false;
                            switch ((Naming.B2BInvoiceDocumentTypeDefinition)item.DocType.Value)
                            {
                                case Naming.B2BInvoiceDocumentTypeDefinition.電子發票:

                                    if (item.InvoiceItem.InvoiceSeller.Seller.OrganizationStatus.Entrusting == true)
                                    {
                                        sb.Clear();
                                        if (item.InvoiceItem.InvoiceSeller.Seller.IsEnterpriseGroupMember())
                                        {
                                            var cert = (new B2BInvoiceManager(mgr)).PrepareSignerCertificate(item.InvoiceItem.InvoiceSeller.Seller);
                                            if (cert != null)
                                            {
                                                bSigned = item.InvoiceItem.SignAndCheckToIssueInvoiceItem(cert, sb);
                                            }
                                        }
                                        else
                                        {
                                            bSigned = item.InvoiceItem.SignAndCheckToIssueInvoiceItem(null, sb);
                                        }
                                    }
                                    break;
                                case Naming.B2BInvoiceDocumentTypeDefinition.發票折讓:
                                    if (item.InvoiceAllowance.InvoiceAllowanceBuyer.Buyer.OrganizationStatus.Entrusting == true)
                                    {
                                        sb.Clear();
                                        if (item.InvoiceAllowance.InvoiceAllowanceBuyer.Buyer.IsEnterpriseGroupMember())
                                        {
                                            var cert = (new B2BInvoiceManager(mgr)).PrepareSignerCertificate(item.InvoiceAllowance.InvoiceAllowanceBuyer.Buyer);
                                            if (cert != null)
                                            {
                                                bSigned = item.InvoiceAllowance.SignAndCheckToIssueInvoiceAllowance(cert, sb);
                                            }
                                        }
                                        else
                                        {
                                            bSigned = item.InvoiceAllowance.SignAndCheckToIssueInvoiceAllowance(null, sb);
                                        }
                                    }
                                    break;
                                case Naming.B2BInvoiceDocumentTypeDefinition.作廢發票:
                                    if (item.DerivedDocument.ParentDocument.InvoiceItem.InvoiceSeller.Seller.OrganizationStatus.Entrusting == true)
                                    {
                                        sb.Clear();
                                        if (item.DerivedDocument.ParentDocument.InvoiceItem.InvoiceSeller.Seller.IsEnterpriseGroupMember())
                                        {
                                            var cert = (new B2BInvoiceManager(mgr)).PrepareSignerCertificate(item.DerivedDocument.ParentDocument.InvoiceItem.InvoiceSeller.Seller);
                                            if (cert != null)
                                            {
                                                bSigned = item.DerivedDocument.ParentDocument.InvoiceItem.SignAndCheckToIssueInvoiceCancellation(cert, sb, item.DocID);
                                            }
                                        }
                                        else
                                        {
                                            bSigned = item.DerivedDocument.ParentDocument.InvoiceItem.SignAndCheckToIssueInvoiceCancellation(null, sb, item.DocID);
                                        }
                                    }
                                    break;
                                case Naming.B2BInvoiceDocumentTypeDefinition.作廢折讓:
                                    if (item!.DerivedDocument?.ParentDocument?.InvoiceAllowance?.InvoiceAllowanceBuyer?.Buyer?.OrganizationStatus?.Entrusting == true)
                                    {
                                        sb.Clear();
                                        if (item.DerivedDocument.ParentDocument.InvoiceAllowance.InvoiceAllowanceBuyer.Buyer.IsEnterpriseGroupMember())
                                        {
                                            var cert = (new B2BInvoiceManager(mgr)).PrepareSignerCertificate(item.DerivedDocument.ParentDocument.InvoiceAllowance.InvoiceAllowanceBuyer.Buyer);
                                            if (cert != null)
                                            {
                                                bSigned = item.DerivedDocument.ParentDocument.InvoiceAllowance.SignAndCheckToIssueAllowanceCancellation(cert, sb, item.DocID);
                                            }
                                        }
                                        else
                                        {
                                            bSigned = item.DerivedDocument.ParentDocument.InvoiceAllowance.SignAndCheckToIssueAllowanceCancellation(null, sb, item.DocID);
                                        }
                                    }
                                    break;
                                case Naming.B2BInvoiceDocumentTypeDefinition.收據:
                                    if (item.ReceiptItem.Seller.OrganizationStatus.Entrusting == true)
                                    {
                                        sb.Clear();
                                        if (item.ReceiptItem.Seller.IsEnterpriseGroupMember())
                                        {
                                            var cert = (new B2BInvoiceManager(mgr)).PrepareSignerCertificate(item.ReceiptItem.Seller);
                                            if (cert != null)
                                            {
                                                bSigned = item.ReceiptItem.SignAndCheckToIssueReceipt(cert, sb);
                                            }
                                        }
                                        else
                                        {
                                            bSigned = item.ReceiptItem.SignAndCheckToIssueReceipt(null, sb);
                                        }
                                    }
                                    break;
                                case Naming.B2BInvoiceDocumentTypeDefinition.作廢收據:
                                    if (item.DerivedDocument.ParentDocument.ReceiptItem.Seller.OrganizationStatus.Entrusting == true)
                                    {
                                        sb.Clear();
                                        if (item.DerivedDocument.ParentDocument.ReceiptItem.Seller.IsEnterpriseGroupMember())
                                        {
                                            var cert = (new B2BInvoiceManager(mgr)).PrepareSignerCertificate(item.DerivedDocument.ParentDocument.ReceiptItem.Seller);
                                            if (cert != null)
                                            {
                                                bSigned = item.DerivedDocument.ParentDocument.ReceiptItem.ReceiptCancellation.SignAndCheckToIssueReceiptCancellation(item.DerivedDocument.ParentDocument.ReceiptItem, cert, sb, item.DocID);
                                            }
                                        }
                                        else
                                        {
                                            bSigned = item.DerivedDocument.ParentDocument.ReceiptItem.ReceiptCancellation.SignAndCheckToIssueReceiptCancellation(item.DerivedDocument.ParentDocument.ReceiptItem, null, sb, item.DocID);
                                        }
                                    }
                                    break;
                                default:
                                    break;
                            }

                            if (bSigned)
                            {
                                transmit(mgr, item);
                            }

                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex);
                        }
                    }
                }
            }
        }

        public async Task MatchDocumentAttachmentAsync()
        {
            using (InvoiceManager mgr = new InvoiceManager())
            {
                var invoices = mgr.GetTable<InvoiceItem>();
                await mgr.DataContext.Database.ExecuteSqlInterpolatedAsync($"EXEC dbo.MatchDocumentAttachment");
            }
        }

        private void transmit(GenericDbContext<ApplicationDbContext> mgr, CDS_Document item)
        {
            //item.MoveToNextStep(mgr);
        }

    }


}
