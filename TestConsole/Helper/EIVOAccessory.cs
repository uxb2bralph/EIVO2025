using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ModelCore.DataEntity;
using ModelCore.InvoiceManagement;
using ModelCore.InvoiceManagement.InvoiceProcess;

namespace TestConsole.Helper
{
    class EIVOAccessory
    {
        static void Main(string[] args)
        {
            //using (ModelSource<InvoiceItem> models = new ModelSource<InvoiceItem>())
            //{
            //    var issuers = models.GetTable<InvoiceIssuerAgent>().Where(a => a.AgentID == 2359)
            //        .Select(a => a.IssuerID);
            //    var items = models.GetTable<InvoiceItem>().Where(i => i.SellerID == 2359 || issuers.Any(a => a == i.SellerID));
            //}

            Console.WriteLine($"開始時間:{DateTime.Now}");
            ProcessC0401();
        }

        private static void ProcessC0401()
        {
            var range = Enumerable.Range(0, 100);
            var tasks = range.Select(i =>
                new Task
                    (
                        () =>
                        {
                            writeToTurnkey(i, 100);
                        }
                    )
                { }).ToArray();
            foreach (var a in tasks)
            {
                a.Start();
            }
            var t = Task.Factory.ContinueWhenAll(tasks, ts =>
            {
                Console.WriteLine($"執行結束:{DateTime.Now}");
            });
            t.Wait();
        }

        static void writeToTurnkey(int idx, int count)
        {
            using (InvoiceManager models = new InvoiceManager())
            {
                F0401Handler c0401 = new F0401Handler(models);
                c0401.WriteToTurnkey(idx, count);
            }
        }
    }
}
