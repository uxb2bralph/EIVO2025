using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Xml;


using ModelCore.DataEntity;
using ModelCore.DocumentManagement;
using ModelCore.Helper;
using ModelCore.Locale;
using ModelCore.Properties;
using CommonLib.Utility;
using System.Security.Cryptography.Pkcs;
using CommonLib.DataAccess;
using CommonLib.Core.Utility;


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

        public void NotifyToProcess()
        {
            using (InvoiceManager mgr = new InvoiceManager())
            {
                var notify = mgr.GetTable<ReplicationNotification>();
                var items = notify.ToList();

                if (items.Count() > 0)
                {
                    var org = mgr.GetTable<Organization>();

                    var toIssue = notify.Select(r => r.DocumentReplication.CDS_Document).Where(d => d.CurrentStep == (int)Naming.InvoiceStepDefinition.待開立);

                    if (toIssue.Count() > 0)
                    {
                        var notifyToIssue = toIssue.Select(t => new NotifyToProcessID { MailToID = t.InvoiceItem.InvoiceSeller.SellerID, Seller = t.InvoiceItem.InvoiceSeller.Organization })
                            .Concat(toIssue.Select(t => new NotifyToProcessID { MailToID = t.DerivedDocument.ParentDocument.InvoiceItem.InvoiceSeller.SellerID, Seller = t.DerivedDocument.ParentDocument.InvoiceItem.InvoiceSeller.Organization }))
                            .Concat(toIssue.Select(t => new NotifyToProcessID { MailToID = t.InvoiceAllowance.InvoiceAllowanceBuyer.BuyerID, Seller = t.InvoiceAllowance.InvoiceAllowanceSeller.Organization }))
                            .Concat(toIssue.Select(t => new NotifyToProcessID { MailToID = t.DerivedDocument.ParentDocument.InvoiceAllowance.InvoiceAllowanceBuyer.BuyerID, Seller = t.DerivedDocument.ParentDocument.InvoiceAllowance.InvoiceAllowanceSeller.Organization }))
                            .Concat(toIssue.Select(t => new NotifyToProcessID { MailToID = (int?)t.ReceiptItem.SellerID, Seller = t.ReceiptItem.Seller }))
                            .Concat(toIssue.Select(t => new NotifyToProcessID { MailToID = (int?)t.DerivedDocument.ParentDocument.ReceiptItem.SellerID, Seller = t.DerivedDocument.ParentDocument.ReceiptItem.Seller }))
                            .Distinct();

                        foreach (var businessID in notifyToIssue)
                        {
                            var item = org.Where(o => o.CompanyID == businessID.MailToID).FirstOrDefault();
                            if (item != null && (item.OrganizationStatus == null || item.OrganizationStatus.Entrusting != true))
                            {
                                //EIVOPlatformFactory.NotifyIssuedA0401(this, new EventArgs<NotifyToProcessID> { Argument = businessID });
                            }
                        }
                    }

                    var toReceive = notify.Select(r => r.DocumentReplication.CDS_Document).Where(d => d.CurrentStep == (int)Naming.InvoiceStepDefinition.待接收);
                    if (toReceive.Count() > 0)
                    {

                        var notifyToReceive = toReceive.Select(t => new NotifyToProcessID { MailToID = t.InvoiceItem.InvoiceBuyer.BuyerID, Seller = t.InvoiceItem.InvoiceSeller.Organization, itemNO = t.InvoiceItem.TrackCode + t.InvoiceItem.No })
                            .Concat(toReceive.Select(t => new NotifyToProcessID { MailToID = t.DerivedDocument.ParentDocument.InvoiceItem.InvoiceBuyer.BuyerID, Seller = t.DerivedDocument.ParentDocument.InvoiceItem.InvoiceSeller.Organization, itemNO = t.DerivedDocument.ParentDocument.InvoiceItem.TrackCode + t.DerivedDocument.ParentDocument.InvoiceItem.No }))
                            .Concat(toReceive.Select(t => new NotifyToProcessID { MailToID = t.InvoiceAllowance.InvoiceAllowanceSeller.SellerID, Seller = t.InvoiceAllowance.InvoiceAllowanceSeller.Organization, itemNO = t.InvoiceAllowance.InvoiceItem.TrackCode + t.InvoiceAllowance.InvoiceItem.No }))
                            .Concat(toReceive.Select(t => new NotifyToProcessID { MailToID = t.DerivedDocument.ParentDocument.InvoiceAllowance.InvoiceAllowanceSeller.SellerID, Seller = t.DerivedDocument.ParentDocument.InvoiceAllowance.InvoiceAllowanceSeller.Organization, itemNO = t.DerivedDocument.ParentDocument.InvoiceAllowance.InvoiceItem.TrackCode + t.DerivedDocument.ParentDocument.InvoiceAllowance.InvoiceItem.No }))
                            .Concat(toReceive.Select(t => new NotifyToProcessID { MailToID = (int?)t.ReceiptItem.BuyerID, Seller = t.ReceiptItem.Seller, itemNO = t.ReceiptItem.No }))
                            .Concat(toReceive.Select(t => new NotifyToProcessID { MailToID = (int?)t.DerivedDocument.ParentDocument.ReceiptItem.BuyerID, Seller = t.DerivedDocument.ParentDocument.ReceiptItem.Seller, itemNO = t.DerivedDocument.ParentDocument.ReceiptItem.No }))
                            .Distinct();

                        foreach (var businessID in notifyToReceive)
                        {
                            var item = org.Where(o => o.CompanyID == businessID.MailToID).FirstOrDefault();
                            //var InvoiceInfo = new NotifyToProcessID { InvoiceItem = item.InvoiceItem, isMail = false };
                            if (item != null && (item.OrganizationStatus == null || item.OrganizationStatus.Entrusting != true))
                            {
                                //EIVOPlatformFactory.NotifyToReceiveItem(this, new EventArgs<NotifyToProcessID> { Argument = businessID });
                            }
                        }
                    }

                    foreach (var i in items)
                    {
                        mgr.DeleteAny<DocumentDispatch>(d => d.DocID == i.DocID && d.TypeID == i.TypeID);
                        mgr.DeleteAny<DocumentReplication>(d => d.DocID == i.DocID && d.TypeID == i.TypeID);
                    }
                }

            }

        }

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
        //                .Join(mgr.GetTable<InvoiceAllowance>(), d => d.DocID, i => i.AllowanceID, (d, i) => i)
        //                .Join(notify, t => t.AllowanceID, s => s.DocID, (t, s) => new _key { a = t.InvoiceAllowanceSeller.SellerID, b = t.InvoiceAllowanceBuyer.BuyerID })
        //                .Join(mgr.GetTable<BusinessRelationship>(), k => k, r => new _key { a = r.MasterID, b = r.RelativeID }, (k, r) => k.b))
        //            .Concat(toIssue
        //                .Join(mgr.GetTable<DerivedDocument>(), t => t.DocID, d => d.DocID, (t, d) => d)
        //                .Join(mgr.GetTable<InvoiceAllowance>(), d => d.SourceID, i => i.AllowanceID, (d, i) => i)
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
        //                .Join(mgr.GetTable<InvoiceAllowance>(), d => d.DocID, i => i.AllowanceID, (d, i) => i)
        //                .Join(notify, t => t.AllowanceID, s => s.DocID, (t, s) => new _key { a = t.InvoiceAllowanceSeller.SellerID, b = t.InvoiceAllowanceBuyer.BuyerID })
        //                .Join(mgr.GetTable<BusinessRelationship>(), k => k, r => new _key { a = r.RelativeID, b = r.MasterID }, (k, r) => k.a))
        //            .Concat(toReceive
        //                .Join(mgr.GetTable<DerivedDocument>(), t => t.DocID, d => d.DocID, (t, d) => d)
        //                .Join(mgr.GetTable<InvoiceAllowance>(), d => d.SourceID, i => i.AllowanceID, (d, i) => i)
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


        //        var org = mgr.GetTable<Organization>();

        //        foreach (var businessID in notifyToIssue)
        //        {
        //            var item = org.Where(o => o.CompanyID == businessID).FirstOrDefault();
        //            if (item != null && (item.OrganizationStatus == null || item.OrganizationStatus.Entrusting != true))
        //            {
        //                EIVOPlatformFactory.NotifyToIssueItem(this, new EventArgs<Organization> { Argument = item });
        //            }
        //        }

        //        foreach (var businessID in notifyToReceive)
        //        {
        //            var item = org.Where(o => o.CompanyID == businessID).FirstOrDefault();
        //            if (item != null && (item.OrganizationStatus == null || item.OrganizationStatus.Entrusting != true))
        //            {
        //                EIVOPlatformFactory.NotifyToReceiveItem(this, new EventArgs<Organization> { Argument = item });
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
        //                        //if (item.InvoiceItem.InvoiceSeller.Organization.OrganizationStatus != null && item.InvoiceItem.InvoiceSeller.Organization.OrganizationStatus.IronSteelIndustry == true)
        //                        //{
        //                        //    fileName = Path.Combine(Settings.Default.A1401Outbound, String.Format("A1401-{0:yyyyMMddHHmmssf}-{1:00000}.xml", DateTime.Now, invoiceCounter++));
        //                        //    item.InvoiceItem.CreateA1401().ConvertToXml().Save(fileName);
        //                        //}
        //                        //else
        //                        {
        //                            fileName = Path.Combine(ModelExtension.Properties.AppSettings.Default.A0401Outbound, $"A0401-{DateTime.Now:yyyyMMddHHmmssf}-{item.InvoiceItem.TrackCode}{item.InvoiceItem.No}.xml");
        //                            item.InvoiceItem.CreateB2BInvoiceMIG().ConvertToXml().Save(fileName);
        //                        }
        //                        break;
        //                    case Naming.DocumentTypeDefinition.E_Allowance:
        //                        //if (item.InvoiceAllowance.InvoiceAllowanceSeller.Organization.OrganizationStatus != null && item.InvoiceAllowance.InvoiceAllowanceSeller.Organization.OrganizationStatus.IronSteelIndustry == true)
        //                        //{
        //                        //    fileName = Path.Combine(Settings.Default.B1401Outbound, String.Format("B1401-{0:yyyyMMddHHmmssf}-{1:00000}.xml", DateTime.Now, allowanceCounter++));
        //                        //    item.InvoiceAllowance.CreateB1401().ConvertToXml().Save(fileName);
        //                        //}
        //                        //else
        //                        {
        //                            fileName = Path.Combine(ModelExtension.Properties.AppSettings.Default.B0401Outbound, $"B0401-{DateTime.Now:yyyyMMddHHmmssf}-{item.InvoiceAllowance.AllowanceNumber}.xml");
        //                            item.InvoiceAllowance.CreateG0401().ConvertToXml().Save(fileName);
        //                        }
        //                        break;
        //                    case Naming.DocumentTypeDefinition.E_InvoiceCancellation:
        //                        fileName = Path.Combine(ModelExtension.Properties.AppSettings.Default.A0501Outbound, String.Format("A0501-{0:yyyyMMddHHmmssf}-{1:00000}.xml", DateTime.Now, cancellationCounter++));
        //                        item.DerivedDocument.ParentDocument.InvoiceItem.CreateF0501().ConvertToXml().Save(fileName);
        //                        break;
        //                    case Naming.DocumentTypeDefinition.E_AllowanceCancellation:
        //                        fileName = Path.Combine(ModelExtension.Properties.AppSettings.Default.B0501Outbound, String.Format("B0501-{0:yyyyMMddHHmmssf}-{1:00000}.xml", DateTime.Now, allowanceCancellationCounter++));
        //                        item.DerivedDocument.ParentDocument.InvoiceAllowance.CreateG0501().ConvertToXml().Save(fileName);
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

        public void CommissionedToReceive()
        {
            using (InvoiceManager mgr = new InvoiceManager())
            {
                var items = mgr.GetTable<CDS_Document>().Where(d => d.CurrentStep == (int)Naming.InvoiceStepDefinition.待接收);

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

                                    if (item.InvoiceItem.InvoiceBuyer.Organization.OrganizationStatus.Entrusting == true)
                                    {
                                        sb.Clear();
                                        if (item.InvoiceItem.InvoiceBuyer.Organization.IsEnterpriseGroupMember())
                                        {
                                            var cert = (new B2BInvoiceManager(mgr)).PrepareSignerCertificate(item.InvoiceItem.InvoiceBuyer.Organization);
                                            if (cert != null)
                                            {
                                                bSigned = item.InvoiceItem.SignAndCheckToReceiveInvoiceItem(cert, sb);
                                            }
                                        }
                                        else
                                        {
                                            bSigned = item.InvoiceItem.SignAndCheckToReceiveInvoiceItem(null, sb);
                                        }
                                        if (bSigned)
                                        {
                                            EIVOTurnkeyFactory.NotifyCommissionedToReceiveA0401(new NotifyToProcessID { DocID = item.DocID });
                                        }
                                    }
                                    break;
                                case Naming.B2BInvoiceDocumentTypeDefinition.發票折讓:
                                    if (item.InvoiceAllowance.InvoiceAllowanceSeller.Organization.OrganizationStatus.Entrusting == true)
                                    {
                                        sb.Clear();
                                        if (item.InvoiceAllowance.InvoiceAllowanceSeller.Organization.IsEnterpriseGroupMember())
                                        {
                                            var cert = (new B2BInvoiceManager(mgr)).PrepareSignerCertificate(item.InvoiceAllowance.InvoiceAllowanceSeller.Organization);
                                            if (cert != null)
                                            {
                                                bSigned = item.InvoiceAllowance.SignAndCheckToReceiveInvoiceAllowance(cert, sb);
                                            }
                                        }
                                        else
                                        {
                                            bSigned = item.InvoiceAllowance.SignAndCheckToReceiveInvoiceAllowance(null, sb);
                                        }
                                        if (bSigned)
                                        {
                                            var businessID = new NotifyToProcessID
                                            {
                                                MailToID = item.InvoiceAllowance.InvoiceAllowanceBuyer.BuyerID,
                                                Seller = item.InvoiceAllowance.InvoiceAllowanceSeller.Organization,
                                                DocID = item.DocID
                                            };
                                            EIVOTurnkeyFactory.NotifyCommissionedToReceive(this, new EventArgs<NotifyToProcessID> { Argument = businessID });
                                        }
                                    }
                                    break;
                                case Naming.B2BInvoiceDocumentTypeDefinition.作廢發票:
                                    if (item.DerivedDocument.ParentDocument.InvoiceItem.InvoiceBuyer.Organization.OrganizationStatus.Entrusting == true)
                                    {
                                        sb.Clear();
                                        if (item.DerivedDocument.ParentDocument.InvoiceItem.InvoiceBuyer.Organization.IsEnterpriseGroupMember())
                                        {
                                            var cert = (new B2BInvoiceManager(mgr)).PrepareSignerCertificate(item.DerivedDocument.ParentDocument.InvoiceItem.InvoiceBuyer.Organization);
                                            if (cert != null)
                                            {
                                                bSigned = item.DerivedDocument.ParentDocument.InvoiceItem.SignAndCheckToReceiveInvoiceCancellation(cert, sb, item.DocID);
                                            }
                                        }
                                        else
                                        {
                                            bSigned = item.DerivedDocument.ParentDocument.InvoiceItem.SignAndCheckToReceiveInvoiceCancellation(null, sb, item.DocID);
                                        }
                                        if (bSigned)
                                        {
                                            var businessID = new NotifyToProcessID
                                            {
                                                DocID = item.DocID
                                            };
                                            EIVOTurnkeyFactory.NotifyCommissionedToReceiveInvoiceCancellation(this, new EventArgs<NotifyToProcessID> { Argument = businessID });
                                        }
                                    }
                                    break;
                                case Naming.B2BInvoiceDocumentTypeDefinition.作廢折讓:
                                    if (item.DerivedDocument.ParentDocument.InvoiceAllowance.InvoiceAllowanceSeller.Organization.OrganizationStatus.Entrusting == true)
                                    {
                                        sb.Clear();
                                        if (item.DerivedDocument.ParentDocument.InvoiceAllowance.InvoiceAllowanceSeller.Organization.IsEnterpriseGroupMember())
                                        {
                                            var cert = (new B2BInvoiceManager(mgr)).PrepareSignerCertificate(item.DerivedDocument.ParentDocument.InvoiceAllowance.InvoiceAllowanceSeller.Organization);
                                            if (cert != null)
                                            {
                                                bSigned = item.DerivedDocument.ParentDocument.InvoiceAllowance.SignAndCheckToReceiveAllowanceCancellation(cert, sb, item.DocID);
                                            }
                                        }
                                        else
                                        {
                                            bSigned = item.DerivedDocument.ParentDocument.InvoiceAllowance.SignAndCheckToReceiveAllowanceCancellation(null, sb, item.DocID);
                                        }
                                        if (bSigned)
                                        {
                                            var businessID = new NotifyToProcessID
                                            {
                                                MailToID = item.DerivedDocument.ParentDocument.InvoiceAllowance.InvoiceAllowanceBuyer.BuyerID,
                                                Seller = item.DerivedDocument.ParentDocument.InvoiceAllowance.InvoiceAllowanceSeller.Organization,
                                                DocID = item.DocID
                                            };
                                            EIVOTurnkeyFactory.NotifyCommissionedToReceive(this, new EventArgs<NotifyToProcessID> { Argument = businessID });
                                        }
                                    }
                                    break;
                                case Naming.B2BInvoiceDocumentTypeDefinition.收據:
                                    if (item.ReceiptItem != null && item.ReceiptItem.Buyer.OrganizationStatus.Entrusting == true)
                                    {
                                        sb.Clear();
                                        if (item.ReceiptItem.Buyer.IsEnterpriseGroupMember())
                                        {
                                            var cert = (new B2BInvoiceManager(mgr)).PrepareSignerCertificate(item.ReceiptItem.Buyer);
                                            if (cert != null)
                                            {
                                                bSigned = item.ReceiptItem.SignAndCheckToReceiveReceipt(cert, sb);
                                            }
                                        }
                                        else
                                        {
                                            bSigned = item.ReceiptItem.SignAndCheckToReceiveReceipt(null, sb);
                                        }
                                        if (bSigned)
                                        {
                                            var businessID = new NotifyToProcessID
                                            {
                                                MailToID = item.ReceiptItem.BuyerID,
                                                Seller = item.ReceiptItem.Seller,
                                                DocID = item.DocID
                                            };
                                            EIVOTurnkeyFactory.NotifyCommissionedToReceive(this, new EventArgs<NotifyToProcessID> { Argument = businessID });
                                        }
                                    }
                                    break;
                                case Naming.B2BInvoiceDocumentTypeDefinition.作廢收據:
                                    if (item.DerivedDocument.ParentDocument.ReceiptItem.Buyer.OrganizationStatus.Entrusting == true)
                                    {
                                        sb.Clear();
                                        if (item.DerivedDocument.ParentDocument.ReceiptItem.Buyer.IsEnterpriseGroupMember())
                                        {
                                            var cert = (new B2BInvoiceManager(mgr)).PrepareSignerCertificate(item.DerivedDocument.ParentDocument.ReceiptItem.Buyer);
                                            if (cert != null)
                                            {
                                                bSigned = item.DerivedDocument.ParentDocument.ReceiptItem.SignAndCheckToReceiveReceiptCancellation(cert, sb, item.DocID);
                                            }
                                        }
                                        else
                                        {
                                            bSigned = item.DerivedDocument.ParentDocument.ReceiptItem.SignAndCheckToReceiveReceiptCancellation(null, sb, item.DocID);
                                        }
                                        if (bSigned)
                                        {
                                            var businessID = new NotifyToProcessID
                                            {
                                                MailToID = item.DerivedDocument.ParentDocument.ReceiptItem.BuyerID,
                                                Seller = item.DerivedDocument.ParentDocument.ReceiptItem.Seller,
                                                DocID = item.DocID
                                            };
                                            EIVOTurnkeyFactory.NotifyCommissionedToReceive(this, new EventArgs<NotifyToProcessID> { Argument = businessID });
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

                                    if (item.InvoiceItem.InvoiceSeller.Organization.OrganizationStatus.Entrusting == true)
                                    {
                                        sb.Clear();
                                        if (item.InvoiceItem.InvoiceSeller.Organization.IsEnterpriseGroupMember())
                                        {
                                            var cert = (new B2BInvoiceManager(mgr)).PrepareSignerCertificate(item.InvoiceItem.InvoiceSeller.Organization);
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
                                    if (item.InvoiceAllowance.InvoiceAllowanceBuyer.Organization.OrganizationStatus.Entrusting == true)
                                    {
                                        sb.Clear();
                                        if (item.InvoiceAllowance.InvoiceAllowanceBuyer.Organization.IsEnterpriseGroupMember())
                                        {
                                            var cert = (new B2BInvoiceManager(mgr)).PrepareSignerCertificate(item.InvoiceAllowance.InvoiceAllowanceBuyer.Organization);
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
                                    if (item.DerivedDocument.ParentDocument.InvoiceItem.InvoiceSeller.Organization.OrganizationStatus.Entrusting == true)
                                    {
                                        sb.Clear();
                                        if (item.DerivedDocument.ParentDocument.InvoiceItem.InvoiceSeller.Organization.IsEnterpriseGroupMember())
                                        {
                                            var cert = (new B2BInvoiceManager(mgr)).PrepareSignerCertificate(item.DerivedDocument.ParentDocument.InvoiceItem.InvoiceSeller.Organization);
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
                                    if (item.DerivedDocument.ParentDocument.InvoiceAllowance.InvoiceAllowanceBuyer.Organization.OrganizationStatus.Entrusting == true)
                                    {
                                        sb.Clear();
                                        if (item.DerivedDocument.ParentDocument.InvoiceAllowance.InvoiceAllowanceBuyer.Organization.IsEnterpriseGroupMember())
                                        {
                                            var cert = (new B2BInvoiceManager(mgr)).PrepareSignerCertificate(item.DerivedDocument.ParentDocument.InvoiceAllowance.InvoiceAllowanceBuyer.Organization);
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

        public void MatchDocumentAttachment()
        {
            using (InvoiceManager mgr = new InvoiceManager())
            {
                var invoices = mgr.GetTable<InvoiceItem>();
                ((EIVOEntityDataContext)invoices.Context).MatchDocumentAttachment();
            }
        }

        private void transmit(GenericManager<EIVOEntityDataContext> mgr, CDS_Document item)
        {
            item.MoveToNextStep(mgr);
        }

    }


}
