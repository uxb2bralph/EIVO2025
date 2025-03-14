using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Xml;
using System.Net;

using CommonLib.Utility;
using ModelCore.Schema.TXN;
using System.Diagnostics;
using System.Threading.Tasks;

using ModelCore.DataEntity;
using ModelCore.Models.ViewModel;
using CommonLib.Helper;
using CommonLib.Core.Helper;
using ModelCore.Helper;
using CommonLib.Core.Utility;

namespace ModelCore.InvoiceManagement
{
    public static class VacantInvoiceNoProcessor
    {
        private static QueuedProcessHandler __Handler;

        static VacantInvoiceNoProcessor()
        {
            lock (typeof(VacantInvoiceNoProcessor))
            {
                if (__Handler == null)
                {
                    __Handler = new QueuedProcessHandler(FileLogger.Logger)
                    {
                        Process = () =>
                        {
                            DateTime calcPeriod = DateTime.Today.AddMonths(-2);
                            int year = calcPeriod.Year;
                            int period = (calcPeriod.Month + 1) / 2;

                            using (TrackNoIntervalManager models = new TrackNoIntervalManager())
                            {
                                //models.SettleVacantInvoiceNo(year, period);
                                var assignments = models.PromptTrackCodeAssignment(year, period);
                                var autoBlankItems = models.GetTable<Organization>()
                                        .Join(models.GetTable<OrganizationExtension>().Where(x => x.AutoBlankTrack == true),
                                            o => o.CompanyID, x => x.CompanyID, (o, x) => o);

                                InquireNoIntervalViewModel viewModel = new InquireNoIntervalViewModel 
                                {
                                    Year = year,
                                    PeriodNo = period,
                                };

                                foreach (var sellerID in autoBlankItems.Select(o => o.CompanyID).ToList())
                                {
                                    try
                                    {
                                        //using (TrackNoIntervalManager working = new TrackNoIntervalManager())
                                        //{
                                        //    working.SettleUnassignedInvoiceNOPeriodically(year, period, sellerID);
                                        //}
                                        viewModel.SellerID = sellerID;
                                        viewModel.Push($"{viewModel.SellerID}-{viewModel.Year}{viewModel.PeriodNo:00}.json");

                                    }
                                    catch (Exception ex)
                                    {
                                        Logger.Error($"結算空白發票號碼失敗，CompanyID: {sellerID}");
                                        Logger.Error(ex);
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
