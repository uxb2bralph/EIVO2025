using CommonLib.Utility;
using JobHelper.Tasks;
using ModelCore.DataEntity;
using ModelCore.InvoiceManagement;
using ModelCore.Schema.TurnKey.Invoice;
using ModelCore.Schema.TXN;
using ModelCore.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Win32;

namespace TestConsole
{
    public class Class1
    {
        protected ValueSet _ValueSet = new ValueSet { };
        protected  class ValueSet
        {
            public int A01 { get; internal set; } = 0;
            public int A02 { get; internal set; } = 1;
        }

        public virtual void Test()
        {
            Console.WriteLine($"{_ValueSet.A01} = {(int)_ValueSet.A01}");
            DateTime date = DateTime.Today;
        }

        public void Test2()
        {
            List<AutomationItem> automation = new List<AutomationItem>();
            List<int> invoiceID = new List<int> { 780806833, 775519684, 776015753, 776233674, 779504647, 781638082 };

            foreach (int id in invoiceID)
            {
                using (TrackNoManager manager = new TrackNoManager(8175))
                {
                    var invoice = manager.GetTable<InvoiceItem>().Where(i => i.InvoiceID == id).FirstOrDefault();
                    if (invoice == null)
                    {
                        continue;
                    }

                    manager.DeleteAny<InvoiceNoAssignment>(n => n.InvoiceID == id);

                    // original data
                    Console.Write($"InvoiceID: {id}, original InvoiceDate: {invoice?.InvoiceDate}, TrackNo: {invoice?.TrackCode}, No:{invoice?.No}");

                    // update invoice date to now
                    invoice.InvoiceDate = DateTime.Today;

                    manager.ApplyInvoiceDate(invoice.InvoiceDate.Value);
                    if (manager.CheckInvoiceNo(invoice))
                    {
                        manager.SubmitChanges();
                        Console.WriteLine($", update InvoiceDate: {invoice?.InvoiceDate}, TrackNo: {invoice?.TrackCode}, No:{invoice?.No}");
                        automation.Add(new AutomationItem
                        {
                            Status = 1,
                            Description = "",
                            Invoice = new AutomationItemInvoice
                            {
                                InvoiceNumber = $"{invoice.TrackCode}{invoice.No}",
                                DataNumber = invoice.InvoicePurchaseOrder.OrderNo,
                                InvoiceDate = String.Format("{0:yyyy/MM/dd}", invoice.InvoiceDate),
                                InvoiceTime = String.Format("{0:HH:mm:ss}", invoice.InvoiceDate),
                            }
                        });
                    }
                    else
                    {
                        Console.WriteLine($", failed to allocate new no for InvoiceID: {id}");
                    }
                }
            }

            Automation auto = new Automation { Item = automation.ToArray() };
            auto.ConvertToXml().Save("G:\\temp\\result.xml");
        }

        public void Test3()
        {
            try
            {
                using (ModelSource models = new ModelSource())
                {
                    List<int> chkItem = new List<int> { 1332321, 1332322 };
                    var items = models.GetTable<InvoiceItem>().Where(i => chkItem.Contains(i.InvoiceID));
                    Console.WriteLine(items.Count());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public void Test4(String[] args)
        {
            if(args?.Length > 0 && int.TryParse(args[0], out int taskID))
            {
                taskID.ProcessWinningNoExcel();
            }
        }

        public void Test5()
        {
            CheckTurnkeyLog.Notify();
        }
    }
}
