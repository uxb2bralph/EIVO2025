using ModelCore.DataEntity;
using ModelCore.Locale;

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CommonLib.Utility;
using CommonLib.Core.Utility;

namespace ProcessorUnit.Execution
{
    public class ProcessRequestExecutorForever : ExecutorForeverBase
    {

        protected ProcessRequestQueue queueItem;
        protected Naming.InvoiceProcessType? appliedProcessType;
        protected override void DoSomething()
        {
            int? taskID = null;
            using (models = new ModelSource<InvoiceItem>())
            {
                try
                {

                    _ = models.DataContext.ApplyProcessRequest(ProcessorUnit.Properties.AppSettings.Default.ProcessorID, (int?)appliedProcessType, ref taskID);

                    if (taskID.HasValue)
                    {
                        queueItem = models.GetTable<ProcessRequestQueue>().Where(q => q.TaskID == taskID).First();
                        ProcessRequestItem();
                    }

                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                    queueItem.ProcessRequest.ExceptionLog = new ExceptionLog
                    {
                        DataContent = ex.Message
                    };
                    queueItem.ProcessRequest.ProcessComplete = DateTime.Now;
                    models.GetTable<ProcessRequestQueue>().DeleteOnSubmit(queueItem);
                    models.SubmitChanges();
                }
            }

            base.DoSomething();
        }

        protected virtual void ProcessRequestItem()
        {

        }

        public virtual void ProcessRequestItem(int taskID) 
        {
            using (models = new ModelSource<InvoiceItem>())
            {
                try
                {
                    var requestItem = models.GetTable<ProcessRequest>()
                        .Where(q => q.TaskID == taskID)
                        .Where(q => q.ProcessType == (int?)appliedProcessType)
                        .FirstOrDefault();

                    if (requestItem!=null)
                    {
                        queueItem = models.GetTable<ProcessRequestQueue>().Where(q => q.TaskID == taskID).First();
                        if (queueItem != null)
                        {
                            ProcessRequestItem();
                        }
                    }

                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                    queueItem.ProcessRequest.ExceptionLog = new ExceptionLog
                    {
                        DataContent = ex.Message
                    };
                    queueItem.ProcessRequest.ProcessComplete = DateTime.Now;
                    models.GetTable<ProcessRequestQueue>().DeleteOnSubmit(queueItem);
                    models.SubmitChanges();
                }
            }

        }
    }
}
