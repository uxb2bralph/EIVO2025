using System;
using System.Collections.Generic;
using System.Data.Linq;
using System.IO;
using System.Linq;

using System.Text;
using System.Threading;
using System.Xml;

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
using System.Web;

namespace InvoiceClient.Agent.TurnkeyProcess
{
    public class D0401ResultWatcher : C0401ResultWatcher
    {
        public D0401ResultWatcher(String fullPath)
            : base(fullPath)
        {

        }

        protected override void processFile(String invFile)
        {
            if (!File.Exists(invFile))
                return;

            String fileName = Path.GetFileName(invFile);
            String fullPath = Path.Combine(_inProgressPath, fileName);
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
                String[] dataNo = JsonConvert.DeserializeObject<String[]>(File.ReadAllText(fullPath));
                List<String> cascadeItems = new List<String>();

                if (dataNo.Any() == true)
                {
                    using (ModelSource models = new ModelSource())
                    {
                        foreach (var item in dataNo)
                        {
                            var allowance = models.GetTable<InvoiceAllowance>()
                                    .Where(i => i.AllowanceNumber == item)
                                    .OrderByDescending(i => i.AllowanceID)
                                    .FirstOrDefault();

                            if (allowance != null)
                            {
                                allowance.CDS_Document.PushLogOnSubmit(models, Naming.InvoiceStepDefinition.MIG_C, Naming.DataProcessStatus.Done);
                                models.SubmitChanges();
                                Console.WriteLine($"Allowance Good:{item}");
                                continue;
                            }

                            cascadeItems.Add(item);
                        }

                        if (cascadeItems.Any())
                        {
                            String storedPath = Path.Combine(_ResponsedPath, $"{Guid.NewGuid()}.json");
                            File.WriteAllText(storedPath, cascadeItems.JsonStringify());
                        }
                    }
                }

                storeFile(fullPath, Path.Combine(Logger.LogDailyPath, fileName));

            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                storeFile(fullPath, Path.Combine(_failedTxnPath, fileName));
            }

        }

    }
}
