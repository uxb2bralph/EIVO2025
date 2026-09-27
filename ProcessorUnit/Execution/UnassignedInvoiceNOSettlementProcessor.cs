using ModelCore.DataEntity;
using ModelCore.Locale;
using ModelCore.Helper;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using CommonLib.Utility;
using ModelCore.InvoiceManagement;
using ClosedXML.Excel;
using System.Net;

using ProcessorUnit.Properties;
using ModelCore.Models.ViewModel;
using CommonLib.Core.Helper;

namespace ProcessorUnit.Execution
{
    public class UnassignedInvoiceNOSettlementProcessor : ExecutorForeverBase
    {
        public UnassignedInvoiceNOSettlementProcessor()
        {

        }

        protected override void DoSomething()
        {
            InquireNoIntervalViewModel viewModel = PersistenceExtensions.Popup<InquireNoIntervalViewModel>(null, ProcessorUnit.Properties.AppSettings.Default.PersistenceModelPath.StoreTargetPath());
            if (viewModel != null)
            {
                using (TrackNoIntervalManager manager = new TrackNoIntervalManager(models))
                {
                    var items = manager.GetTable<InvoiceTrackCode>()
                        .Where(t => t.Year == viewModel.Year && t.PeriodNo == viewModel.PeriodNo)
                        .Join(manager.GetTable<InvoiceTrackCodeAssignment>(), t => t.TrackID, a => a.TrackID, (t, a) => a)
                        .Select(a => a.SellerID);

                    if (viewModel.SellerID.HasValue)
                    {
                        manager.ExecuteCommand("exec SettleUnassignedInvoiceNOPeriodically @Year, @PeriodNo, @SellerID", new { Year = viewModel!.Year!.Value, PeriodNo = viewModel!.PeriodNo!.Value, SellerID = viewModel.SellerID });
                        manager.ExecuteCommand("exec SettlementInvoiceNo @SellerID, @Year, @PeriodNo", new { SellerID = viewModel.SellerID, Year = viewModel.Year.Value, PeriodNo = viewModel.PeriodNo.Value });

                        if (viewModel.BranchRelation == true)
                        {
                            var agency = manager.GetTable<InvoiceIssuerAgent>().Where(a => a.AgentID == viewModel.SellerID);
                            items = items.Where(c => agency.Any(a => a.IssuerID == c));

                            foreach (var orgItem in items)
                            {
                                manager.ExecuteCommand("exec SettleUnassignedInvoiceNOPeriodically @Year, @PeriodNo, @SellerID", new { Year = viewModel!.Year!.Value, PeriodNo = viewModel!.PeriodNo!.Value, SellerID = orgItem });
                                manager.ExecuteCommand("exec SettlementInvoiceNo @SellerID, @Year, @PeriodNo", new { SellerID = orgItem, Year = viewModel.Year.Value, PeriodNo = viewModel.PeriodNo.Value });
                            }
                        }
                    }
                    else 
                    {
                        foreach (var orgItem in items)
                        {
                            manager.ExecuteCommand("exec SettleUnassignedInvoiceNOPeriodically @Year, @PeriodNo, @SellerID", new { Year = viewModel!.Year!.Value, PeriodNo = viewModel!.PeriodNo!.Value, SellerID = orgItem });
                            manager.ExecuteCommand("exec SettlementInvoiceNo @SellerID, @Year, @PeriodNo", new { SellerID = orgItem, Year = viewModel.Year.Value, PeriodNo = viewModel.PeriodNo.Value });
                        }
                    }
                }
            }

            base.DoSomething();
        }

    }
}
