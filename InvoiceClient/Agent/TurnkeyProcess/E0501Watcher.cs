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
                    ModelState.AddModelError("StartNo", $"本區段營業人({appliedItem.InvoiceTrackCodeAssignment.Seller.ReceiptNo})已使用!!");
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

                models.GetTable<InvoiceTrackCodeAssignment>().Add(codeAssignment);
            }

            model = new InvoiceNoInterval
            {
                LockID = seller.OrganizationCustomSetting?.Settings.E0501InitialLock == Naming.Truth.True ? 1 : null,
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

                    codeAssignment.Assignment = headquarterAssignment;
                }
            }

            models.SubmitChanges();

            if (headquarterAssignment != null && seller.OrganizationCustomSetting?.Settings.BranchInvoiceNoAssignments?.Length > 0)
            {
                AssignBranchNoIntervals(models, seller, headquarterAssignment, model);
            }
        }

        /// <summary>
        /// 依主機構之批次配號設定（OrganizationCustomSetting.Settings.BranchInvoiceNoAssignments），
        /// 自 <paramref name="mainInterval"/> 起號依序切出號段分派給各分支機構。
        /// 每筆設定取 本組數 × 50 個號碼，建立該分支機構之配號區間（並將其字軌指派掛在主機構配號 AssignmentID 下，
        /// E0401 與配號查詢皆以此歸戶）；未分派完的號碼留在主機構原區間，全數分派完畢則移除該區間。
        /// </summary>
        /// <param name="headquarterAssignment">本號段之主機構配號（E0401 表頭號段），供分支機構字軌指派歸戶</param>
        /// <param name="mainInterval">主機構本次取得之配號區間（已扣除保留本組數）</param>
        private void AssignBranchNoIntervals(ModelSource models, Organization seller,
            InvoiceNoMainAssignment headquarterAssignment, InvoiceNoInterval mainInterval)
        {
            var branchSettings = seller.OrganizationCustomSetting?.Settings.BranchInvoiceNoAssignments;
            if (branchSettings == null || branchSettings.Length == 0)
                return;

            if (mainInterval.EndNo < mainInterval.StartNo)
            {
                Logger.Warn($"E0501主機構批次配號({seller.ReceiptNo})：區間號碼錯誤({mainInterval.StartNo}~{mainInterval.EndNo})，未分派!!");
                return;
            }

            // 分派游標：自主機構區間起號開始，依設定順序逐一切出號段。
            var startNo = mainInterval.StartNo;
            var endNo = mainInterval.EndNo;

            foreach (var branchSetting in branchSettings)
            {
                var receiptNo = branchSetting.ReceiptNo.GetEfficientString();
                var booklets = branchSetting.Booklets ?? 0;
                if (receiptNo == null || booklets <= 0)
                    continue;

                var branch = models.GetTable<Organization>().Where(o => o.ReceiptNo == receiptNo).FirstOrDefault();
                if (branch == null || branch.CompanyID == seller.CompanyID)
                {
                    Logger.Warn($"E0501主機構批次配號({seller.ReceiptNo})：分支機構({receiptNo})不存在或為主機構本身，略過!!");
                    continue;
                }

                // 統編為全域查詢，須確認確實為本主機構名下之分支機構，避免號段配給無關營業人。
                if (!seller.BranchRelation.Any(r => r.IssuerID == branch.CompanyID))
                {
                    Logger.Warn($"E0501主機構批次配號({seller.ReceiptNo})：({receiptNo})非本主機構之分支機構，略過!!");
                    continue;
                }

                // 每本 50 號；剩餘號碼不足本次配號時停止分派，避免破壞後續設定順序。
                var count = booklets * 50;
                if (startNo + count - 1 > endNo)
                {
                    Logger.Warn($"E0501主機構批次配號({seller.ReceiptNo})：剩餘號碼({endNo - startNo + 1})不足分支機構({receiptNo})所需({count})，自此停止分派!!");
                    break;
                }

                // 分支機構之字軌指派須掛在主機構配號下（AssignmentID 指向 InvoiceNoMainAssignment）；
                // 配號區間之 FK 為 (TrackID, SellerID)，故須先存檔字軌指派再新增區間。
                var branchCodeAssignment = models.GetTable<InvoiceTrackCodeAssignment>()
                    .Where(t => t.TrackID == mainInterval.TrackID && t.SellerID == branch.CompanyID)
                    .FirstOrDefault();
                if (branchCodeAssignment == null)
                {
                    branchCodeAssignment = new InvoiceTrackCodeAssignment
                    {
                        TrackID = mainInterval.TrackID,
                        SellerID = branch.CompanyID,
                        AssignmentID = headquarterAssignment.AssignmentID,
                    };
                    models.GetTable<InvoiceTrackCodeAssignment>().Add(branchCodeAssignment);
                    models.SubmitChanges();
                }
                else if (branchCodeAssignment.AssignmentID == null)
                {
                    branchCodeAssignment.AssignmentID = headquarterAssignment.AssignmentID;
                    models.SubmitChanges();
                }

                models.GetTable<InvoiceNoInterval>().Add(new InvoiceNoInterval
                {
                    TrackID = mainInterval.TrackID,
                    SellerID = branch.CompanyID,
                    StartNo = startNo,
                    EndNo = startNo + count - 1,
                    LockID = branchSetting.InitialLock == Naming.Truth.True ? 1 : null,
                });

                startNo += count;
            }

            if (startNo > mainInterval.StartNo)
            {
                if (startNo > endNo)
                {
                    // 號碼已全數分派給分支機構，主機構不留區間。
                    models.GetTable<InvoiceNoInterval>().Remove(mainInterval);
                }
                else
                {
                    mainInterval.StartNo = startNo;
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
                                        // 單筆配號為一個異動單元：各自使用獨立 context 並包在交易內，
                                        // 一筆失敗只退回該筆，不影響同一檔案的其他配號。
                                        // （EF 的變更追蹤器不會隨 Rollback 還原，故不可與外層共用的 models 同一個 context。）
                                        using (ModelSource itemModels = new ModelSource())
                                        using (var tran = itemModels.EnterTransaction())
                                        {
                                            try
                                            {
                                                var itemSeller = itemModels.GetTable<Organization>()
                                                    .Where(o => o.CompanyID == item.SellerID).FirstOrDefault();
                                                if (itemSeller == null)
                                                {
                                                    throw new Exception($"營業人({item.ReceiptNo})資料錯誤!!");
                                                }

                                                CommitItem(itemModels, item, itemSeller);
                                                tran.Commit();
                                            }
                                            catch (Exception ex)
                                            {
                                                tran.Rollback();
                                                Logger.Error($"{data.OuterXml}\r\n{ex}");
                                            }
                                        }
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
