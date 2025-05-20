using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.IO;
using System.Linq;

using System.Text;
using System.Threading;
using System.Xml;
using System.Reflection;


using InvoiceClient.Helper;
using InvoiceClient.Properties;
using ModelCore.DataEntity;
using ModelCore.Locale;
using ModelCore.Schema.EIVO.B2B;
using ModelCore.Schema.TXN;
using ModelCore.Helper;
using Newtonsoft.Json;
using CommonLib.Core.Utility;
using CommonLib.Utility;
using ModelCore.Models.ViewModel;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using ModelCore.InvoiceManagement.InvoiceProcess;



namespace InvoiceClient.Agent.TurnkeyProcess
{
    public class E0501Watcher : InvoiceWatcher
    {
        public E0501Watcher(String fullPath)
            : base(fullPath)
        {
            //_ResponsedPath = fullPath + "(Response)";
            //_ResponsedPath.CheckStoredPath();
        }

        ModelStateDictionary ModelState { get; } = new ModelStateDictionary();
        private void CheckInput(ModelSource models, UploadInvoiceTrackCodeModel viewModel)
        {
            var table = models.GetTable<InvoiceNoInterval>();
            var year = viewModel.Year + 1911;
            viewModel.TrackID = models.GetTable<InvoiceTrackCode>().Where(t => t.Year == year && t.PeriodNo == viewModel.PeriodNo && t.TrackCode == viewModel.TrackCode)
                    .FirstOrDefault()?.TrackID;

            if (!viewModel.TrackID.HasValue)
            {
                ModelState.AddModelError("TrackID", "字軌未設定!!");
            }

            if (!viewModel.SellerID.HasValue)
            {
                ModelState.AddModelError("SellerID", "營業人錯誤!!");
            }

            if (!viewModel.StartNo.HasValue || !(viewModel.StartNo >= 0 && viewModel.StartNo < 100000000))
            {
                ModelState.AddModelError("StartNo", "起號非8位整數!!");
            }
            else if (!viewModel.EndNo.HasValue || !(viewModel.EndNo >= 0 && viewModel.EndNo < 100000000))
            {
                ModelState.AddModelError("EndNo", "迄號非8位整數!!");
            }
            else if (viewModel.EndNo <= viewModel.StartNo || ((viewModel.EndNo - viewModel.StartNo + 1) % 50 != 0))
            {
                ModelState.AddModelError("StartNo", "不符號碼大小順序與差距為50之倍數原則!!");
            }
            else
            {
                if (table.Any(t => t.TrackID == viewModel.TrackID
                    && ((t.EndNo <= viewModel.EndNo && t.EndNo >= viewModel.StartNo) || (t.StartNo <= viewModel.EndNo && t.StartNo >= viewModel.StartNo) || (t.StartNo <= viewModel.StartNo && t.EndNo >= viewModel.StartNo) || (t.StartNo <= viewModel.EndNo && t.EndNo >= viewModel.EndNo))))
                {
                    var appliedItem = table
                            .Where(t => t.TrackID == viewModel.TrackID
                                && ((t.EndNo <= viewModel.EndNo && t.EndNo >= viewModel.StartNo) || (t.StartNo <= viewModel.EndNo && t.StartNo >= viewModel.StartNo) || (t.StartNo <= viewModel.StartNo && t.EndNo >= viewModel.StartNo) || (t.StartNo <= viewModel.EndNo && t.EndNo >= viewModel.EndNo)))
                            .First();
                    ModelState.AddModelError("StartNo", $"本區段營業人({appliedItem.InvoiceTrackCodeAssignment.Organization.ReceiptNo})已使用!!");
                }
            }
        }

        public void CommitItem(ModelSource models, InvoiceNoIntervalViewModel viewModel, Organization seller)
        {
            InvoiceNoInterval? model = null;
            if (seller.OrganizationCustomSetting?.Settings.DisableE0501AutoUpdate == Naming.Truth.True)
                return;

            var codeAssignment = models.GetTable<InvoiceTrackCodeAssignment>().Where(t => t.SellerID == viewModel.SellerID && t.TrackID == viewModel.TrackID).FirstOrDefault();
            if (codeAssignment == null)
            {
                codeAssignment = new InvoiceTrackCodeAssignment
                {
                    SellerID = viewModel.SellerID!.Value,
                    TrackID = viewModel.TrackID!.Value
                };

                models.GetTable<InvoiceTrackCodeAssignment>().InsertOnSubmit(codeAssignment);
            }

            model = new InvoiceNoInterval
            {
                LockID = seller.OrganizationCustomSetting?.Settings.E0501InitialLock == Naming.Truth.False ? (int?)null : 1,
            };
            codeAssignment.InvoiceNoInterval.Add(model);

            model.StartNo = viewModel.StartNo!.Value;
            if (seller.OrganizationCustomSetting?.Settings.E0501ReservedBooklets > 0)
            {
                if ((viewModel.EndNo - viewModel.StartNo + 1) / 50 > seller.OrganizationCustomSetting?.Settings.E0501ReservedBooklets)
                {
                    var reservedInterval = new InvoiceNoInterval
                    {
                        LockID = 1,
                        EndNo = viewModel.EndNo!.Value,
                    };
                    codeAssignment.InvoiceNoInterval.Add(reservedInterval);

                    model.EndNo = viewModel.EndNo.Value - seller.OrganizationCustomSetting.Settings.E0501ReservedBooklets.Value * 50;
                    reservedInterval.StartNo = model.EndNo + 1;
                }
            }
            else
            {
                model.EndNo = viewModel.EndNo!.Value;
            }

            InvoiceNoMainAssignment? headquarterAssignment = null;
            if (seller.IsMasterBranch())
            {
                headquarterAssignment = codeAssignment.InvoiceNoMainAssignment.Where(m => m.StartNo == viewModel.StartNo).FirstOrDefault();
                if (headquarterAssignment == null)
                {
                    headquarterAssignment = new InvoiceNoMainAssignment
                    {
                        InvoiceTrackCodeAssignment = codeAssignment,
                        StartNo = viewModel.StartNo.Value,
                        EndNo = viewModel.EndNo!.Value,
                    };

                    codeAssignment.HeadquarterNoAssignment = headquarterAssignment;
                }
            }

            models.SubmitChanges();
        }


        protected override void processFile(String invFile)
        {
            if (!File.Exists(invFile))
                return;

            String fileName = Path.GetFileName(invFile);
            String fullPath = Path.Combine(_inProgressPath, fileName);
            String backupPath = Path.Combine(Logger.LogDailyPath, fileName);

            try
            {
                File.Move(invFile, fullPath);
            }
            catch (Exception ex)
            {
                Logger.Error($"while processing move {invFile} => {fullPath}\r\n{ex}");
                return;
            }

            try
            {
                XmlDocument doc = new XmlDocument();
                doc.Load(fullPath);

                List<XmlElement> items = new List<XmlElement>();

                if (doc.DocumentElement?["InvoicePack"]?["InvoiceAssignNo"] != null)
                {
                    using (ModelSource models = new ModelSource())
                    {
                        foreach (XmlElement data in doc.DocumentElement["InvoicePack"].GetElementsByTagName("InvoiceAssignNo"))
                        {
                            UploadInvoiceTrackCodeModel item = new UploadInvoiceTrackCodeModel { };
                            try
                            {
                                item.ReceiptNo = data["Ban"].InnerText;
                                item.TrackCode = data["InvoiceTrack"].InnerText;
                                int yearNo = int.Parse(data["YearMonth"].InnerText);
                                item.Year = (short)(yearNo / 100);
                                item.PeriodNo = (yearNo % 100) / 2;
                                item.StartNo = int.Parse(data["InvoiceBeginNo"].InnerText);
                                item.EndNo = int.Parse(data["InvoiceEndNo"].InnerText);

                                var seller = models.GetTable<Organization>().Where(o => o.ReceiptNo == item.ReceiptNo).FirstOrDefault();
                                if (seller != null)
                                {
                                    item.SellerID = seller.CompanyID;
                                    ModelState.Clear();
                                    CheckInput(models, item);

                                    if(seller.OrganizationExtension.ExpirationDate.HasValue && seller.OrganizationExtension.ExpirationDate.Value < DateTime.Today)
                                    {
                                        ModelState.AddModelError("ReceiptNo", "營業人已停用!!");
                                    }

                                    if (ModelState.IsValid)
                                    {
                                        CommitItem(models, item, seller);
                                    }
                                    else
                                    {
                                        String warning = $"{data.OuterXml}\r\n{ModelState.ErrorMessage()}";
                                        Logger.Warn(warning);
                                        //$"E0501({backupPath}):\r\n{warning}".PushToLineNotify();
                                    }
                                }

                            }
                            catch (Exception ex)
                            {
                                Logger.Error($"{data.OuterXml}\r\n{ex}");
                            }
                        }
                    }
                }
                storeFile(fullPath, backupPath);

            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                storeFile(fullPath, Path.Combine(_failedTxnPath, fileName));
            }

        }

        protected override void processComplete()
        {

        }
    }
}
