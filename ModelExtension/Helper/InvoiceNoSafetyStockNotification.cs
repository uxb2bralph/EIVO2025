using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using System.Net;
using System.Threading.Tasks;

using CommonLib.Utility;
using ModelCore.Schema.TXN;

using ModelCore.DataEntity;
using ModelCore.Models.ViewModel;
using ModelCore.InvoiceManagement;
using CommonLib.Helper;
using CommonLib.Core.Utility;

namespace ModelCore.Helper
{
    public static class InvoiceNoSafetyStockNotification
    {
        private static QueuedProcessHandler __Handler;

        static InvoiceNoSafetyStockNotification()
        {
            lock (typeof(InvoiceNoSafetyStockNotification))
            {
                if (__Handler == null)
                {
                    __Handler = new QueuedProcessHandler(FileLogger.Logger)
                    {
                        MaxWaitingCount = 1,
                        Process = () =>
                        {

                            InquireNoIntervalViewModel viewModel = new InquireNoIntervalViewModel
                            {
                                Year = DateTime.Today.Year,
                                PeriodNo = (DateTime.Today.Month + 1) / 2
                            };
                            
                            using (TrackNoIntervalManager models = new TrackNoIntervalManager())
                            {
                                //models.SettleVacantInvoiceNo(year, period);
                                var assignments =
                                    models.PromptTrackCodeAssignment(viewModel.Year.Value, viewModel.PeriodNo.Value)
                                        .Where(a => a.Organization.OrganizationExtension.InvoiceNoSafetyStock.HasValue);

                                OrganizationViewModel orgModel = new OrganizationViewModel { };

                                foreach (var item in assignments.ToList())
                                {
                                    viewModel.SellerID = item.SellerID;
                                    var items = viewModel.InquireInvoiceNoInterval(models);
                                    if (items.Any())
                                    {
                                        var invoiceNoStock = items.ToList().Sum(i => i.EndNo + 1 - i.CurrentAllocatingNo());
                                        if (invoiceNoStock < item.Organization.OrganizationExtension.InvoiceNoSafetyStock)
                                        {
                                            orgModel.CompanyID = item.SellerID;
                                            EIVOTurnkeyFactory.NotifyLowerInvoiceNoStock(orgModel);
                                        }
                                    }

                                }
                            }
                        }
                    };
                }
            }
        }

        public static int ResetBusyCount()
        {
            return __Handler.ResetBusyCount();
        }

        public static void Notify()
        {
            __Handler.Notify();
        }
    }
}
