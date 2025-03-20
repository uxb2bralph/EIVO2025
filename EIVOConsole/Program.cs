using ModelCore.DataEntity;
using ModelCore.InvoiceManagement;
using ModelCore.Locale;
using ModelCore.Models;
using ModelCore.Helper;
using CommonLib.Utility;
using ModelCore.InvoiceManagement.InvoiceProcess;

namespace EIVOConsole
{
    internal static class Program
    {
        /// <summary>
        ///  The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            // To customize application configuration such as set high DPI settings or default font,
            // see https://aka.ms/applicationconfiguration.
            ApplicationConfiguration.Initialize();

            Start(args);
        }

        static void Start(String[] args)
        {
            if (args.Length > 0)
            {
                switch (args[0])
                {
                    case "001":
                        ProcessVacantInvoiceNo(args.Length > 2 ? args[1] : null, args.Length > 2 ? args[2] : null, args.Length > 3 ? args[3] : null);
                        break;
                    default:
                        Console.WriteLine(@"
Use command:
    001    執行前期空白發票號結算
");
                        break;
                }
            }
            else
            {
                Application.Run(new MyApplicationContext(() =>
                {
                    //Console.WriteLine("Hello, World!");
                    EIVOTurnkeyFactory.Notify();
                }));
            }
        }
        private static void ProcessVacantInvoiceNo(string? strYear,string? strPeriod,string? strSellerId)
        {
            DateTime calcPeriod = DateTime.Today.AddMonths(-2);
            int year = calcPeriod.Year;
            int period = (calcPeriod.Month + 1) / 2;
            int? sellerID = null;

            if (int.TryParse(strYear, out int v))
            {
                year = v;
            }
            if (int.TryParse(strPeriod, out v))
            {
                period = v;
            }

            if (int.TryParse(strSellerId, out v))
            {
                sellerID = v;
            }

            void SaveE0402(InvoiceTrackCodeAssignment assignment)
            {
                if(assignment.UnassignedInvoiceNo.Any())
                {
                    E0402Handler.WriteToTurnkey(assignment.BuildE0402());
                }
            }

            using (TrackNoIntervalManager models = new TrackNoIntervalManager())
            {
                foreach(var assignment in models.SettleUnassignedInvoiceNOPeriodically(year, period, sellerID))
                {
                    SaveE0402(assignment);
                }

                if (sellerID.HasValue)
                {
                    foreach (var item in models.GetTable<InvoiceIssuerAgent>().Where(r => r.AgentID == sellerID))
                    {
                        foreach(var assignment in models.SettleUnassignedInvoiceNOPeriodically(year, period, item.IssuerID))
                        {
                            SaveE0402(assignment);
                        }
                    }
                }
            }
        }

    }

    class MyApplicationContext : ApplicationContext
    {
        public MyApplicationContext(Action action)
        {
            if (action != null)
            {
                action();
            }
        }
    }
}